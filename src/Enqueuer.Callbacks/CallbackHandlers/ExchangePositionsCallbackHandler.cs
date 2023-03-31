using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class ExchangePositionsCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private const int MembersPerPage = 10;

    private readonly IQueueService _queueService;
    private readonly ILogger<EnqueueCallbackHandler> _logger;

    public ExchangePositionsCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IQueueService queueService, ILogger<EnqueueCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html);
        }

        return HandleAsyncInternal(callback);
    }

    private async Task HandleAsyncInternal(Callback callback)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        callback.CallbackData.TargetChatId = queue.GroupId;
        var receiver = queue.Members.FirstOrDefault(m => m.UserId == callback.From.Id);
        if (receiver == null) 
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                $"You're not participating in queue '<b>{queue.Name}</b>'.",
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        if (callback.CallbackData.TargetUserId.HasValue)
        {
            await HandleCallbackWithTargetUserAsync(queue, callback, receiver);
            return;
        }

        var replyButtons = BuildKeyboardMarkup(queue, callback, receiver);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            "Select the user with whom you want to request a position swap.",
            ParseMode.Html,
            replyMarkup: replyButtons);
    }

    private async Task HandleCallbackWithTargetUserAsync(Queue queue, Callback callback, QueueMember receiver)
    {
        var sender = queue.Members.FirstOrDefault(m => m.UserId == callback.CallbackData!.TargetUserId!.Value);
        if (sender == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                $"<b>{sender.User.FullName}</b> has left the '<b>{queue.Name}</b>' queue. Swap was not executed.",
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        if (callback.CallbackData!.UserAgreement.HasValue)
        {
            await HandleCallbackWithAgreementAsync(queue, callback, sender, receiver);
            return;
        }

        var replyButtons = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
        {
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, "Lets swap!", userAgreement: true) },
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, "Deny", userAgreement: false) },
        });

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId/* callback.From.Id*/,
            $"<b>{receiver.User.FullName}</b> wants to switch their's '{receiver.Position}' position with your in the '<b>{queue.Name}</b>' queue.",
            ParseMode.Html,
            replyMarkup: replyButtons);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            $"The position swap request has been sent. Waiting for recepient's response!",
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData));
    }

    private async Task HandleCallbackWithAgreementAsync(Queue queue, Callback callback, QueueMember requester, QueueMember responder)
    {
        if (requester.Position != callback.CallbackData.QueueData.Position)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                $"<b>{requester.User.FullName}</b> was changed. Another position swap request is needed.",
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        if (callback.CallbackData.HasUserAgreement)
        {
            await _queueService.SwitchMembersPositionsAsync(queue.Id, requester.UserId, responder.UserId, CancellationToken.None);
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                $"Successfully swapped positions with <b>{requester.User.FullName}</b>!",
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            await TelegramBotClient.SendTextMessageAsync(
                callback.CallbackData.TargetUserId/* requester.UserId*/,
                $"Successfully swapped positions with <b>{responder.User.FullName}</b>!",
                ParseMode.Html);

            return;
        }

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            $"Positions switch with <b>{requester.User.FullName}</b> was rejected.",
            ParseMode.Html);

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId /*requester.UserId*/,
            $"<b>{requester.User.FullName}</b> denied position switch.",
            ParseMode.Html);
    }

    private InlineKeyboardMarkup BuildKeyboardMarkup(Queue queue, Callback callback, QueueMember exchangeRequester)
    {
        var currentPage = callback.CallbackData!.Page ?? 0;
        var numberOfMembersToSkip = MembersPerPage * currentPage;
        var replyButtons = new List<InlineKeyboardButton[]>(MembersPerPage + 3);

        var membersToDisplay = queue.Members
            .Where(m => m.UserId != exchangeRequester.UserId)
            .OrderBy(m => m.Position)
            .Skip(numberOfMembersToSkip)
            .Take(MembersPerPage);

        foreach (var member in membersToDisplay)
        {
            replyButtons.Add(new InlineKeyboardButton[] { GetExchangeRequestButton(member, exchangeRequester.Position) });
        }

        var membersLeft = queue.Members.Count - numberOfMembersToSkip - replyButtons.Count - 1;
        if (numberOfMembersToSkip > 0 &&  membersLeft > 0)
        {
            replyButtons.Add(new InlineKeyboardButton[]
            {
                GetAnotherPageButton(callback.CallbackData, currentPage - 1, "<<<"),
                GetAnotherPageButton(callback.CallbackData, currentPage + 1, ">>>")
            });
        }
        else if (numberOfMembersToSkip > 0 && membersLeft == 0)
        {
            replyButtons.Add(new InlineKeyboardButton[]
            {
                GetAnotherPageButton(callback.CallbackData, currentPage - 1, "Previous")
            });
        }
        else if (membersLeft > 0)
        {
            replyButtons.Add(new InlineKeyboardButton[]
            {
                GetAnotherPageButton(callback.CallbackData, currentPage + 1, "Next")
            });
        }

        replyButtons.Add(new InlineKeyboardButton[] { GetRefreshButton(callback.CallbackData) });
        replyButtons.Add(new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) });

        return new InlineKeyboardMarkup(replyButtons);
    }

    private InlineKeyboardButton GetExchangeRequestButton(QueueMember queueMember, int sourcePosition)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.ExchangePositionsCommand,
            TargetUserId = queueMember.UserId,
            QueueData = new QueueData()
            {
                QueueId = queueMember.QueueId,
                Position = sourcePosition,
            },
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData($"{queueMember.Position}) {queueMember.User.FullName}", serializedCallbackData);
    }

    private InlineKeyboardButton GetExchangeRequestResponseButton(QueueMember queueMember, string buttonText, bool userAgreement)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.ExchangePositionsCommand,
            TargetUserId = queueMember.UserId,
            QueueData = new QueueData()
            {
                QueueId = queueMember.QueueId,
                Position = queueMember.Position,
            },
            UserAgreement = userAgreement
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}

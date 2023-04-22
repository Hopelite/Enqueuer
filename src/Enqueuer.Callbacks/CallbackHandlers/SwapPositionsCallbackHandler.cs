using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Core.Constants;
using Enqueuer.Core.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Serialization;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class SwapPositionsCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private const int MembersPerPage = 10;

    private readonly IQueueService _queueService;
    private readonly ILogger<EnqueueCallbackHandler> _logger;

    public SwapPositionsCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, IMessageProvider messageProvider, IQueueService queueService, ILogger<EnqueueCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message),
                ParseMode.Html);
        }

        return HandleAsyncInternal(callback);
    }

    private async Task HandleAsyncInternal(Callback callback)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData!.QueueId, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        callback.CallbackData.TargetChatId = queue.GroupId; // Temporary workaround

        var userInQueue = queue.Members.FirstOrDefault(m => m.UserId == callback.From.Id);
        if (userInQueue == null) 
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserDoesNotParticipate_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData));

            return;
        }

        if (callback.CallbackData.TargetUserId.HasValue)
        {
            await HandleCallbackWithTargetUserAsync(queue, callback, userInQueue);
            return;
        }

        var replyButtons = BuildKeyboardMarkup(queue, callback, userInQueue);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SelectUserToSwapWith_Message),
            ParseMode.Html,
            replyMarkup: replyButtons);
    }

    private async Task HandleCallbackWithTargetUserAsync(Queue queue, Callback callback, QueueMember receiver)
    {
        var targetUser = queue.Members.FirstOrDefault(m => m.UserId == callback.CallbackData!.TargetUserId!.Value);
        if (targetUser == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserHasLeftTheQueue_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData!));

            return;
        }

        if (callback.CallbackData!.UserAgreement.HasValue)
        {
            await HandleCallbackWithAgreementAsync(queue, callback, targetUser, receiver);
            return;
        }

        var replyButtons = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
        {
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_AgreeToSwap_Button), userAgreement: true) },
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_RefuseToSwap_Button), userAgreement: false) },
        });

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId!,
            MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserWantsToSwapWithYou_Message, receiver.User.FullName, receiver.Position, queue.Name),
            ParseMode.Html,
            replyMarkup: replyButtons);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapRequestHasBeenSent_Message, targetUser.User.FullName),
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData));
    }

    private async Task HandleCallbackWithAgreementAsync(Queue queue, Callback callback, QueueMember sender, QueueMember responder)
    {
        if (!callback.CallbackData!.QueueData!.Position.HasValue)
        {
            _logger.LogError("Position in {Sender}'s request to swap with {Responder} in queue {Queue} was null.", sender.User.FullName, responder.User.FullName, queue.Name);
            return;
        }

        if (sender.Position != callback.CallbackData.QueueData.Position)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_RequestersPositionHasChanged_Message, sender.User.FullName),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData));

            return;
        }

        if (callback.CallbackData.HasUserAgreement)
        {
            await _queueService.SwapMembersPositionsAsync(queue.Id, sender.UserId, responder.UserId, CancellationToken.None);
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SuccessfullySwappedPositions_Message, sender.User.FullName),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData));

            await TelegramBotClient.SendTextMessageAsync(
                callback.CallbackData.TargetUserId!,
                MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SuccessfullySwappedPositions_Message, responder.User.FullName),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(GetReturnToQueueButton(callback.CallbackData)));

            return;
        }

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapWasSuccessfulyRefused_Message, sender.User.FullName),
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData));

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId!,
            MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapRequestWasRefusedByUser_Message, responder.User.FullName),
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(GetReturnToQueueButton(callback.CallbackData)));
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
                GetAnotherPageButton(callback.CallbackData, currentPage - 1, MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_ListOfMembers_OnlyPreviousPageAvailable_Button))
            });
        }
        else if (membersLeft > 0)
        {
            replyButtons.Add(new InlineKeyboardButton[]
            {
                GetAnotherPageButton(callback.CallbackData, currentPage + 1, MessageProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_ListOfMembers_OnlyNextPageAvailable_Button))
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

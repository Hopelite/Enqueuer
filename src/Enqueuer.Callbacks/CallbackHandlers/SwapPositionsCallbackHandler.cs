using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class SwapPositionsCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private const int MembersPerPage = 10;
    private readonly IQueueService _queueService;

    public SwapPositionsCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IQueueService queueService)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _queueService = queueService;
    }

    protected override Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callback, cancellationToken);
    }

    private async Task HandleAsyncInternal(Callback callback, CancellationToken cancellationToken)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData!.QueueId, includeMembers: true, cancellationToken);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        callback.CallbackData.TargetChatId = queue.GroupId; // Temporary workaround

        var userInQueue = queue.Members.FirstOrDefault(m => m.UserId == callback.From.Id);
        if (userInQueue == null) 
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserDoesNotParticipate_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        if (callback.CallbackData.TargetUserId.HasValue)
        {
            await HandleCallbackWithTargetUserAsync(queue, callback, userInQueue, cancellationToken);
            return;
        }

        var replyButtons = BuildKeyboardMarkup(queue, callback, userInQueue);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SelectUserToSwapWith_Message, MessageParameters.None),
            ParseMode.Html,
            replyMarkup: replyButtons,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackWithTargetUserAsync(Queue queue, Callback callback, QueueMember receiver, CancellationToken cancellationToken)
    {
        var targetUser = queue.Members.FirstOrDefault(m => m.UserId == callback.CallbackData!.TargetUserId!.Value);
        if (targetUser == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserHasLeftTheQueue_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData!),
                cancellationToken: cancellationToken);

            return;
        }

        if (callback.CallbackData!.UserAgreement.HasValue)
        {
            await HandleCallbackWithAgreementAsync(queue, callback, targetUser, receiver, cancellationToken);
            return;
        }

        var replyButtons = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
        {
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_AgreeToSwap_Button, MessageParameters.None), userAgreement: true) },
            new InlineKeyboardButton[] { GetExchangeRequestResponseButton(receiver, LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_RefuseToSwap_Button, MessageParameters.None), userAgreement: false) },
        });

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId!,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_UserWantsToSwapWithYou_Message, new MessageParameters(receiver.User.FullName, receiver.Position.ToString(), queue.Name)),
            ParseMode.Html,
            replyMarkup: replyButtons,
            cancellationToken: cancellationToken);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapRequestHasBeenSent_Message, new MessageParameters(targetUser.User.FullName)),
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData),
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackWithAgreementAsync(Queue queue, Callback callback, QueueMember sender, QueueMember responder, CancellationToken cancellationToken)
    {
        if (!callback.CallbackData!.QueueData!.Position.HasValue)
        {
            throw new Exception($"Position in {sender.User.FullName}'s request to swap with {responder.User.FullName} in queue {queue.Name} was null.");
        }

        if (sender.Position != callback.CallbackData.QueueData.Position)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_RequestersPositionHasChanged_Message, new MessageParameters(sender.User.FullName)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        if (callback.CallbackData.HasUserAgreement)
        {
            await _queueService.SwapMembersPositionsAsync(queue.Id, sender.UserId, responder.UserId, cancellationToken);
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SuccessfullySwappedPositions_Message, new MessageParameters(sender.User.FullName)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            await TelegramBotClient.SendTextMessageAsync(
                callback.CallbackData.TargetUserId!,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SuccessfullySwappedPositions_Message, new MessageParameters(responder.User.FullName)),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(GetReturnToQueueButton(callback.CallbackData)),
                cancellationToken: cancellationToken);

            return;
        }

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapWasSuccessfulyRefused_Message, new MessageParameters(sender.User.FullName)),
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData),
            cancellationToken: cancellationToken);

        await TelegramBotClient.SendTextMessageAsync(
            callback.CallbackData.TargetUserId!,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_SwapRequestWasRefusedByUser_Message, new MessageParameters(responder.User.FullName)),
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(GetReturnToQueueButton(callback.CallbackData)),
            cancellationToken: cancellationToken);
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
                GetAnotherPageButton(callback.CallbackData, currentPage - 1, LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_ListOfMembers_OnlyPreviousPageAvailable_Button, MessageParameters.None))
            });
        }
        else if (membersLeft > 0)
        {
            replyButtons.Add(new InlineKeyboardButton[]
            {
                GetAnotherPageButton(callback.CallbackData, currentPage + 1, LocalizationProvider.GetMessage(CallbackMessageKeys.SwapPositionsCallbackHandler.Callback_SwapPositions_ListOfMembers_OnlyNextPageAvailable_Button, MessageParameters.None))
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
            Command = CallbackCommands.ExchangePositionsCommand,
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
            Command = CallbackCommands.ExchangePositionsCommand,
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

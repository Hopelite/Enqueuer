using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class GetChatCallbackHandler : CallbackHandlerBase
{
    private readonly IQueueService _queueService;
    private readonly IGroupService _groupService;
    private readonly ILogger<EnqueueMeCallbackHandler> _logger;

    public GetChatCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IGroupService groupService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, ILogger<EnqueueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _queueService = queueService;
        _groupService = groupService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData == null || !callbackContext.CallbackData!.TargetChatId.HasValue)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callbackContext, cancellationToken);
    }

    private async Task HandleAsyncInternal(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (!await _groupService.DoesGroupExist(callbackContext.CallbackData.TargetChatId!.Value))
        {
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_ChatHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnButton(),
                cancellationToken: cancellationToken);

            return;
        }

        var queues = await _queueService.GetGroupQueuesAsync(callbackContext.CallbackData!.TargetChatId.Value, cancellationToken);
        var responseMessage = (queues.Count == 0
            ? LocalizationProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.Callback_GetChat_ChatHasNoQueues_Message, MessageParameters.None)
            : LocalizationProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.Callback_GetChat_DisplayQueuesList_Message, MessageParameters.None))
                + Environment.NewLine + LocalizationProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.Callback_GetChat_DisplayQueueList_PostScriptum_Message, MessageParameters.None); // TODO: refactor this line

        var replyMarkup = BuildReplyMarkup(queues, callbackContext.CallbackData, callbackContext.CallbackData.TargetChatId.Value);
        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private InlineKeyboardMarkup BuildReplyMarkup(List<Queue> chatQueues, CallbackData callbackData, long chatId)
    {
        var replyButtons = new InlineKeyboardButton[chatQueues.Count + 2][];
        for (var i = 0; i < chatQueues.Count; i++)
        {
            var newCallbackData = new CallbackData()
            {
                Command = CallbackCommands.GetQueueCommand,
                TargetChatId = chatId,
                QueueData = new QueueData()
                {
                    QueueId = chatQueues[i].Id,
                },
            };

            var serializedCallbackData = DataSerializer.Serialize(newCallbackData);
            replyButtons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"'{chatQueues[i].Name}'", serializedCallbackData) };
        }

        replyButtons[^2] = new InlineKeyboardButton[] { GetRefreshButton(callbackData) };
        replyButtons[^1] = new InlineKeyboardButton[] { GetReturnButton() };
        return new InlineKeyboardMarkup(replyButtons);
    }

    private InlineKeyboardButton GetReturnButton()
    {
        var callbackData = new CallbackData()
        {
            Command = CallbackCommands.ListChatsCommand,
        };

        var serializedCallbackData = DataSerializer.Serialize(callbackData);
        return InlineKeyboardButton.WithCallbackData(LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
    }
}

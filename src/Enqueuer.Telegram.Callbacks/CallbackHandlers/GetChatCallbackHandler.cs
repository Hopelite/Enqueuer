using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Helpers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class GetChatCallbackHandler : CallbackHandlerBase
{
    private readonly IQueueService _queueService;
    private readonly IGroupService _groupService;
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly ILogger<EnqueueMeCallbackHandler> _logger;

    public GetChatCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IGroupService groupService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, ILogger<EnqueueMeCallbackHandler> logger)
        : base(telegramBotClient, localizationProvider)
    {
        _queueService = queueService;
        _groupService = groupService;
        _dataSerializer = dataSerializer;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData == null || !callbackContext.CallbackData.TargetChatId.HasValue)
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
                replyMarkup: ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider).WithReturnToChatListButton().Build(),
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
        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider);
        foreach (var queue in chatQueues)
        {
            replyMarkup.FromNewRow()
                .WithOpenQueueButton(chatId, queue);
        }

        return replyMarkup.FromNewRow()
            .WithRefreshButton(callbackData)
            .FromNewRow()
                .WithReturnToChatListButton()
            .Build();
    }
}

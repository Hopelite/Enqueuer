using System.Collections.Generic;
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

public class GetChatCallbackHandler : CallbackHandlerBase
{
    private readonly IQueueService _queueService;
    private readonly IGroupService _groupService;
    private readonly ILogger<EnqueueMeCallbackHandler> _logger;

    public GetChatCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IGroupService groupService, ICallbackDataSerializer dataSerializer, IMessageProvider messageProvider, ILogger<EnqueueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _queueService = queueService;
        _groupService = groupService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData == null || !callback.CallbackData!.TargetChatId.HasValue)
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
        if (!await _groupService.DoesGroupExist(callback.CallbackData!.TargetChatId.Value))
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.ChatHasBeenDeleted_Message),
                replyMarkup: GetReturnButton());

            return;
        }

        var queues = await _queueService.GetGroupQueuesAsync(callback.CallbackData!.TargetChatId.Value, CancellationToken.None);
        var responseMessage = (queues.Count == 0
            ? MessageProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.Callback_GetChat_ChatHasNoQueues_Message)
            : MessageProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ListQueues_Message))
                + MessageProvider.GetMessage(CallbackMessageKeys.GetChatCallbackHandler.Callback_GetChat_DisplayQueueList_PostScriptum_Message);

        var replyMarkup = BuildReplyMarkup(queues, callback.CallbackData, callback.CallbackData.TargetChatId.Value);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private InlineKeyboardMarkup BuildReplyMarkup(List<Queue> chatQueues, CallbackData callbackData, long chatId)
    {
        var replyButtons = new InlineKeyboardButton[chatQueues.Count + 2][];
        for (int i = 0; i < chatQueues.Count; i++)
        {
            var newCallbackData = new CallbackData()
            {
                Command = CallbackConstants.GetQueueCommand,
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
            Command = CallbackConstants.ListChatsCommand,
        };

        var serializedCallbackData = DataSerializer.Serialize(callbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.Return_Button), serializedCallbackData);
    }
}

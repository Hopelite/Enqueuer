﻿using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers with return to chat and return to queue buttons.
/// </summary>
public abstract class CallbackHandlerBaseWithReturnToQueueButton : CallbackHandlerBaseWithReturnToChatButton
{
    protected CallbackHandlerBaseWithReturnToQueueButton(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
    }

    /// <summary>
    /// Gets the return to queue button.
    /// </summary>
    protected InlineKeyboardButton GetReturnToQueueButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.GetQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = callbackData.QueueData,
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.Return_Button), serializedCallbackData);
    }
}

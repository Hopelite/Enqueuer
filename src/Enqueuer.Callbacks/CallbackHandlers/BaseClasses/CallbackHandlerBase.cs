﻿using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.Exceptions;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers.
/// </summary>
public abstract class CallbackHandlerBase : ICallbackHandler
{
    protected readonly ITelegramBotClient TelegramBotClient;
    protected readonly IDataSerializer DataSerializer;
    protected readonly IMessageProvider MessageProvider;

    protected CallbackHandlerBase(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider)
    {
        TelegramBotClient = telegramBotClient;
        DataSerializer = dataSerializer;
        MessageProvider = messageProvider;
    }

    public Task HandleAsync(Callback callback)
    {
        try
        {
            return HandleAsyncImplementation(callback);
        }
        catch (MessageNotModifiedException)
        {
            return TelegramBotClient.AnswerCallbackQueryAsync(callback.Id, MessageProvider.GetMessage(CallbackMessageKeys.EverythingIsUpToDate_Message));
        }
    }

    /// <summary>
    /// Contains the implementation of <paramref name="callback"/> handling.
    /// </summary>
    protected abstract Task HandleAsyncImplementation(Callback callback);

    /// <summary>
    /// Creates the refresh button with the <paramref name="callbackData"/>.
    /// </summary>
    protected InlineKeyboardButton GetRefreshButton(CallbackData callbackData)
    {
        var serializedCallbackData = DataSerializer.Serialize(callbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.EverythingIsUpToDate_Message), serializedCallbackData);
    }
}

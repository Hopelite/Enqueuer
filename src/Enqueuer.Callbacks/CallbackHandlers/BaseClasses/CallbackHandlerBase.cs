using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core.Exceptions;
using Enqueuer.Core.TextProviders;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers.
/// </summary>
public abstract class CallbackHandlerBase : ICallbackHandler
{
    protected readonly ITelegramBotClient TelegramBotClient;
    protected readonly ICallbackDataSerializer DataSerializer;
    protected readonly IMessageProvider MessageProvider;

    protected CallbackHandlerBase(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, IMessageProvider messageProvider)
    {
        TelegramBotClient = telegramBotClient;
        DataSerializer = dataSerializer;
        MessageProvider = messageProvider;
    }

    public async Task HandleAsync(Callback callback, CancellationToken cancellationToken)
    {
        try
        {
            await HandleAsyncImplementation(callback, cancellationToken);
            await TelegramBotClient.AnswerCallbackQueryAsync(callback.Id);
        }
        catch (MessageNotModifiedException)
        {
            await TelegramBotClient.AnswerCallbackQueryAsync(callback.Id, MessageProvider.GetMessage(CallbackMessageKeys.Callback_EverythingIsUpToDate_Message));
        }
    }

    /// <summary>
    /// Contains the implementation of <paramref name="callback"/> handling.
    /// </summary>
    protected abstract Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the refresh button with the <paramref name="callbackData"/>.
    /// </summary>
    protected InlineKeyboardButton GetRefreshButton(CallbackData callbackData)
    {
        var serializedCallbackData = DataSerializer.Serialize(callbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.Callback_RefreshMessage_Button), serializedCallbackData);
    }

    /// <summary>
    /// 
    /// </summary>
    protected InlineKeyboardButton GetAnotherPageButton(CallbackData previousCallbackData, int page, string buttonText)
    {
        var newCallbackData = new CallbackData
        {
            Command = previousCallbackData.Command,
            Page = page,
            UserAgreement = previousCallbackData.UserAgreement,
            QueueData = previousCallbackData.QueueData,
            TargetChatId = previousCallbackData.TargetChatId,
            TargetUserId = previousCallbackData.TargetUserId,
        };

        var serializedCallbackData = DataSerializer.Serialize(newCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}

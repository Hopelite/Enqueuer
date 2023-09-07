using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Exceptions;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers.
/// </summary>
public abstract class CallbackHandlerBase : ICallbackHandler
{
    protected readonly ITelegramBotClient TelegramBotClient;
    protected readonly ICallbackDataSerializer DataSerializer;
    protected readonly ILocalizationProvider LocalizationProvider;

    protected CallbackHandlerBase(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
    {
        TelegramBotClient = telegramBotClient;
        DataSerializer = dataSerializer;
        LocalizationProvider = localizationProvider;
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
            await TelegramBotClient.AnswerCallbackQueryAsync(
                callback.Id,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_EverythingIsUpToDate_Message, MessageParameters.None),
                cancellationToken: cancellationToken);
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
        return InlineKeyboardButton.WithCallbackData(LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_RefreshMessage_Button, MessageParameters.None), serializedCallbackData);
    }

    /// <summary>
    /// Creates a button for pagination.
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

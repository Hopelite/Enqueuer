using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Exceptions;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

/// <summary>
/// Contains basic implementation for callback handlers.
/// </summary>
public abstract class CallbackHandlerBase : ICallbackHandler
{
    protected readonly ITelegramBotClient TelegramBotClient;
    protected readonly ILocalizationProvider LocalizationProvider;

    protected CallbackHandlerBase(ITelegramBotClient telegramBotClient, ILocalizationProvider localizationProvider)
    {
        TelegramBotClient = telegramBotClient;
        LocalizationProvider = localizationProvider;
    }

    public async Task HandleAsync(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        try
        {
            await HandleAsyncImplementation(callbackContext, cancellationToken);
        }
        catch (MessageNotModifiedException)
        {
            await TelegramBotClient.AnswerCallbackQueryAsync(
                callbackContext.QueryId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_EverythingIsUpToDate_Message, MessageParameters.None),
                cancellationToken: cancellationToken);
        }
        catch (OutdatedCallbackException)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        finally
        {
            await TelegramBotClient.AnswerCallbackQueryAsync(callbackContext.QueryId, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Contains the implementation of <paramref name="callbackContext"/> handling.
    /// </summary>
    protected abstract Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken);
}

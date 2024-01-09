using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.Helpers.Markup;

internal static class MarkupBuilderExtensions
{
    /// <summary>
    /// Creates a button for pagination.
    /// </summary>
    public static IMarkupBuilder WithAnotherPageButton(this IMarkupBuilder markupBuilder, CallbackData previousCallbackData, int page, string buttonText)
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

        return markupBuilder.Add(serializer =>
        {
            var serializedCallbackData = serializer.Serialize(newCallbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        });
    }

    /// <summary>
    /// Creates a return to queue button.
    /// </summary>
    public static IMarkupBuilder WithReturnToQueueButton(this IMarkupBuilder markupBuilder, CallbackData callbackData, string buttonText)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = callbackData.QueueData,
        };

        return markupBuilder.Add(serializer =>
        {
            var serializedCallbackData = serializer.Serialize(callbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        });
    }

    /// <summary>
    /// Adds the refresh button with the <paramref name="callbackData"/>.
    /// </summary>
    public static IMarkupBuilder WithRefreshButton(this IMarkupBuilder markupBuilder, CallbackData callbackData, string buttonText)
    {
        return markupBuilder.Add(serializer =>
        {
            var serializedCallbackData = serializer.Serialize(callbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        });

        //var serializedCallbackData = _dataSerializer.Serialize(callbackData);
        //var button = InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(CallbackMessageKeys.Callback_RefreshMessage_Button, MessageParameters.None), serializedCallbackData);
        //_row.Enqueue(button);
        //return this;
    }
}

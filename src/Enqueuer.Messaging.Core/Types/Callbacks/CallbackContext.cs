using System.Diagnostics.CodeAnalysis;
using Enqueuer.Messaging.Core.Helpers;
using Enqueuer.Messaging.Core.Types.Common;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using User = Enqueuer.Messaging.Core.Types.Common.User;

namespace Enqueuer.Messaging.Core.Types.Callbacks;

public class CallbackContext
{
    /// <summary>
    /// The unique identifier for this callback's query.
    /// </summary>
    public string QueryId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the message within the boundaries of the chat the callback is attached to.
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// The user who sent this callback.
    /// </summary>
    public User Sender { get; set; } = null!;

    /// <summary>
    /// The group from which this callback was sent.
    /// </summary>
    public Group Chat { get; set; } = null!;

    /// <summary>
    /// Deserialized callback data.
    /// </summary>
    public CallbackData CallbackData { get; set; } = null!;

    /// <summary>
    /// Tries to create a <paramref name="callbackContext"/> from <paramref name="callbackQuery"/>.
    /// </summary>
    /// <returns>True, if <paramref name="callbackContext"/> created successfully; otherwise false.</returns>
    public static bool TryCreate(CallbackQuery callbackQuery, [NotNullWhen(returnValue: true)] out CallbackContext? callbackContext)
    {
        callbackContext = null;
        if (callbackQuery.From == null || callbackQuery.Message == null || callbackQuery.Data == null)
        {
            return false;
        }

        var callbackData = JsonConvert.DeserializeObject<CallbackData?>(callbackQuery.Data);
        if (callbackData == null)
        {
            return false;
        }

        callbackContext = new CallbackContext()
        {
            QueryId = callbackQuery.Id,
            MessageId = callbackQuery.Message.MessageId,
            CallbackData = callbackData,
        };

        callbackContext.Sender = new User
        {
            Id = callbackQuery.From.Id,
            FirstName = callbackQuery.From.FirstName,
            LastName = callbackQuery.From.LastName,
            InterfaceLanguage = ChatConfigurationHelper.GetCultureNameFromIetfTag(callbackQuery.From.LanguageCode),
        };

        callbackContext.Chat = new Group
        {
            Id = callbackQuery.Message.Chat.Id,
            Title = callbackQuery.Message.Chat.Title,
            Type = (ChatType)callbackQuery.Message.Chat.Type,
        };

        return true;
    }
}

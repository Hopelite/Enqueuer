using Enqueuer.Data;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks;

/// <summary>
/// Represents an incoming Telegram callback.
/// </summary>
public class Callback
{
    /// <summary>
    /// Unique identifier for this query.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Sender.
    /// </summary>
    public User From { get; set; }

    /// <summary>
    /// Description with the callback button that originated the query.
    /// </summary>
    public Message Message { get; set; }

    /// <summary>
    /// Optional. Deserialized callback data if exists.
    /// </summary>
    public CallbackData? CallbackData { get; set; }

    public Callback(CallbackQuery callbackQuery, CallbackData? callbackData)
    {
        Id = callbackQuery.Id;
        From = callbackQuery.From;
        CallbackData = callbackData;
        Message = callbackQuery.Message!;
    }
}

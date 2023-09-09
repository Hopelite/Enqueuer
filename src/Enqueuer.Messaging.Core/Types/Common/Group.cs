namespace Enqueuer.Messaging.Core.Types.Common;

/// <summary>
/// Represents a Telegram group.
/// </summary>
public class Group : Chat
{
    /// <summary>
    /// Optional. The title of this group. Null, if the chat is private.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The type of this chat.
    /// </summary>
    public ChatType Type { get; set; }

    // TODO: remove
    public static implicit operator Telegram.Bot.Types.Chat(Group group) => new()
    {
        Id = group.Id,
        Title = group.Title,
        Type = (Telegram.Bot.Types.Enums.ChatType)group.Type,
    };
}

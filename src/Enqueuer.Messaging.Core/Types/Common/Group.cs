using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messaging.Core.Types.Common;

public class Group : Chat
{
    public string? Title { get; set; }

    public ChatType Type { get; set; }

    public static implicit operator Telegram.Bot.Types.Chat(Group group) => new Telegram.Bot.Types.Chat()
    {
        Id = group.Id,
        Title = group.Title,
        Type = group.Type,
    };
}

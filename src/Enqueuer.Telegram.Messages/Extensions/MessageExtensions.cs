using Enqueuer.Messaging.Core.Types.Messages;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Messages.Extensions;

/// <summary>
/// Contains extension methods for <see cref="Message"/>.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Checks if <paramref name="messageContext"/> was sent in private chat.
    /// </summary>
    /// <param name="messageContext"><see cref="Message"/> to check.</param>
    /// <returns>True, if message came from private chat; false otherwise.</returns>
    public static bool IsFromPrivateChat(this MessageContext messageContext)
    {
        return (int)messageContext.Chat.Type == (int)ChatType.Private;
    }
}

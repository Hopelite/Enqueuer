using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Bot.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="Message"/>.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Checks if <paramref name="message"/> was sent in private chat.
        /// </summary>
        /// <param name="message"><see cref="Message"/> to check.</param>
        /// <returns>True, if message came from private chat; false otherwise.</returns>
        public static bool IsPrivateChat(this Message message)
        {
            return message.Chat.Type == ChatType.Private;
        }
    }
}

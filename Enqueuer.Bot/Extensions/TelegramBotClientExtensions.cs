using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Bot.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="ITelegramBotClient"/>.
    /// </summary>
    public static class TelegramBotClientExtensions
    {
        /// <summary>
        /// Sends <see cref="Message"/> with text, that opeations in private chats currently are not supported.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message"><see cref="Message"/> to get <see cref="Chat"/> from.</param>
        /// <returns><see cref="Message"/> sent.</returns>
        public static async Task<Message> SendUnsupportedOperationMessage(this ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(
                message.Chat,
                "Currently bot does not support private chats. Please, add it to your group chat to get access to it's functionality.");
        }
    }
}

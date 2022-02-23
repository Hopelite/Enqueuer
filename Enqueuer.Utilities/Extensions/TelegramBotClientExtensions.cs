using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Utilities.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="ITelegramBotClient"/>.
    /// </summary>
    public static class TelegramBotClientExtensions
    {
        /// <summary>
        /// Sends <see cref="Message"/> with text, that commands in private chats are not supported.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message"><see cref="Message"/> to get <see cref="Chat"/> from.</param>
        /// <returns><see cref="Message"/> sent.</returns>
        public static async Task<Message> SendUnsupportedOperationMessage(this ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(
                message.Chat,
                "Bot does not support commands in private chats except '<b>/start</b>'. Please, use interface it provides.",
                ParseMode.Html);
        }
    }
}

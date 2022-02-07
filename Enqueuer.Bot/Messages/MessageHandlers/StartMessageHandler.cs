using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Bot.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/start' command.
    /// </summary>
    public class StartMessageHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public string Command => "/start";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/start' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message">Incoming <see cref="Message"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(
                message.Chat,
                "Hello there! I'm the <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "To get list of commands and help, write '<b>/help</b>'.",
                ParseMode.Html);
        }
    }
}

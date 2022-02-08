using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Bot.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/help' command.
    /// </summary>
    public class HelpMessageHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public string Command => "/help";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/help' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message">Incoming <see cref="Message"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(
                message.Chat,
                "There is list of available commands with short description:\n"
                + "<b>/start</b> - get introducing message\n"
                + "<b>/help</b> - get bot help\n"
                + "<b>/queue</b> - list chat queues or get info about one of them\n"
                + "<b>/createqueue</b> - create new queue\n"
                + "<b>/enqueue</b> - add yourself to the end of the queue or specified position\n"
                + "<b>/dequeue</b> - remove yourself from queue\n"
                + "<b>/removequeue</b> - delete queue\n"
                + "To get slightly more detailed info about one of them, write appopriate command.",
                ParseMode.Html);
        }
    }
}

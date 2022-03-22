using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class HelpMessageHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public string Command => "/help";

        /// <inheritdoc/>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(
                message.Chat,
                "Here is the list of available commands with short description:\n"
                + "<b>/start</b> - get introducing message\n"
                + "<b>/help</b> - get bot help\n"
                + "<b>/queue</b> - list chat queues or get info about one of them\n"
                + "<b>/createqueue</b> - create new queue\n"
                + "<b>/enqueue</b> - add yourself to the end of a queue or to a specified position\n"
                + "<b>/dequeue</b> - remove yourself from a queue\n"
                + "<b>/removequeue</b> - delete a queue\n"
                + "To get slightly more detailed info about one of them, write the appropriate command.",
                ParseMode.Html);
        }
    }
}

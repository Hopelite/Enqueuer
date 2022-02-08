using Enqueuer.Bot.Configuration;
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
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartMessageHandler"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IBotConfiguration"/> with bot configuration.</param>
        public StartMessageHandler(IBotConfiguration botConfiguration)
        {
            this.botConfiguration = botConfiguration;
        }

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
                + "To get list of commands, write '<b>/help</b>'.\n"
                + "<i>Please, message this guy (@hopelite) to get help, give feedback or report bug.</i>\n"
                + $"\n<i>Bot version: {this.botConfiguration.BotVersion}</i>",
                ParseMode.Html);
        }
    }
}

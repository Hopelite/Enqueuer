using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/>.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Gets command this message handler responds to.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Handles incoming <paramref name="message"/>.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message">Incoming <see cref="Message"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in response.</returns>
        public Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message);
    }
}

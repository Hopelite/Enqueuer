using System.Threading.Tasks;
using Enqueuer.Messages.MessageHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Messages
{
    /// <summary>
    /// Distributes messages to message handlers.
    /// </summary>
    public interface IMessageDistributor
    {
        /// <summary>
        /// Distributes <paramref name="message"/> to one of the <see cref="IMessageHandler"/> with specified command if exists.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message"><see cref="Message"/> to distribute.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task DistributeMessageAsync(ITelegramBotClient telegramBotClient, Message message);
    }
}

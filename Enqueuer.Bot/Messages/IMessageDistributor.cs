using System.Threading.Tasks;
using Enqueuer.Bot.Messages.MessageHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Bot.Messages
{
    /// <summary>
    /// Distributes messages to message handlers.
    /// </summary>
    public interface IMessageDistributor
    {
        /// <summary>
        /// Adds <see cref="IMessageHandler"/> to message handlers <see cref="IMessageDistributor"/> will spread incoming messages.
        /// </summary>
        /// <param name="messageHandler"></param>
        public void AddMessageHandler(IMessageHandler messageHandler);

        /// <summary>
        /// Distributes <paramref name="message"/> to one of <see cref="IMessageHandler"/> with specified command if exists.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message"><see cref="Message"/> to distribute.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task DistributeMessageAsync(ITelegramBotClient telegramBotClient, Message message);
    }
}

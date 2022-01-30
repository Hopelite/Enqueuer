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
        /// Distributes <paramref name="message"/> 
        /// </summary>
        /// <param name="telegramBotClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task DistributeMessageAsync(ITelegramBotClient telegramBotClient, Message message);
    }
}

using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Bot.Exceptions;
using Enqueuer.Bot.Extensions;
using Enqueuer.Bot.Factories;
using Enqueuer.Bot.Messages.MessageHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Bot.Messages
{
    /// <inheritdoc/>
    public class MessageDistributor : IMessageDistributor
    {
        private readonly SortedDictionary<string, IMessageHandler> messageHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor"/> class.
        /// </summary>
        public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>(
                messageHandlersFactory
                .CreateMessageHandlers()
                .ToDictionary(messageHandler => messageHandler.Command));
        }

        /// <inheritdoc/>
        public void AddMesssageHandler(IMessageHandler messageHandler)
        {
            if (!this.messageHandlers.TryAdd(messageHandler.Command, messageHandler))
            {
                throw new MessageHandlerAlreadyInUseException(messageHandler);
            }
        }

        /// <inheritdoc/>
        public async Task DistributeMessageAsync(ITelegramBotClient telegramBotClient, Message message)
        {
            var command = message.Text?.SplitToWords()[0];
            if (command is not null && this.messageHandlers.TryGetValue(command, out IMessageHandler messageHandler))
            {
                await messageHandler.HandleMessageAsync(telegramBotClient, message);
            }
        }
    }
}

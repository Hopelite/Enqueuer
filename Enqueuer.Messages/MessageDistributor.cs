using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Messages.Exceptions;
using Enqueuer.Messages.Factories;
using Enqueuer.Messages.MessageHandlers;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace Enqueuer.Messages
{
    /// <inheritdoc/>
    public class MessageDistributor : IMessageDistributor
    {
        private readonly SortedDictionary<string, IMessageHandler> messageHandlers;
        private readonly ILogger<IMessageDistributor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> to log info.</param>
        public MessageDistributor(ILogger<IMessageDistributor> logger)
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>();
            this.logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor"/> class and adds <see cref="IMessageHandler"/> using <paramref name="messageHandlersFactory"/>.
        /// </summary>
        /// <param name="messageHandlersFactory"><see cref="IMessageHandlersFactory"/> which provides distibutor with <see cref="IMessageHandler"/>.</param>
        /// <param name="logger"><see cref="ILogger"/> to log info.</param>
        public MessageDistributor(IMessageHandlersFactory messageHandlersFactory, ILogger<IMessageDistributor> logger)
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>(
                messageHandlersFactory
                .CreateMessageHandlers()
                .ToDictionary(messageHandler => messageHandler.Command));
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void AddMessageHandler(IMessageHandler messageHandler)
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
            if (command is not null)
            {
                var messageHandler = this.messageHandlers.FirstOrDefault(pair => command.Contains(pair.Key));
                if (messageHandler.Value is not null)
                {
                    var sentMessage = await messageHandler.Value.HandleMessageAsync(telegramBotClient, message);
                    this.logger.LogInformation($"Sent message '{sentMessage.Text}' to {sentMessage.Chat.Title ?? "@" + sentMessage.Chat.Username}");
                }
            }
        }
    }
}

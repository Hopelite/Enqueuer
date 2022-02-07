﻿using System.Linq;
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
        public MessageDistributor()
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor"/> class and adds <see cref="IMessageHandler"/> using <paramref name="messageHandlersFactory"/>.
        /// </summary>
        /// <param name="messageHandlersFactory"><see cref="IMessageHandlersFactory"/> which provides distibutor with <see cref="IMessageHandler"/>.</param>
        public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>(
                messageHandlersFactory
                .CreateMessageHandlers()
                .ToDictionary(messageHandler => messageHandler.Command));
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
                    await messageHandler.Value.HandleMessageAsync(telegramBotClient, message);
                }
            }
        }
    }
}

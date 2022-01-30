﻿using System.Collections.Generic;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Enqueuer.Bot.Messages.MessageHandlers;

namespace Enqueuer.Bot.Factories
{
    /// <inheritdoc/>
    public class MessageHandlersFactory : IMessageHandlersFactory
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Chat> chatRepository;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="chatRepository">Chat repository to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public MessageHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Chat> chatRepository,
            IRepository<Queue> queueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.chatRepository = chatRepository;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public IEnumerable<IMessageHandler> CreateMessageHandlers()
        {
            return new IMessageHandler[]
            {
                new CreateQueueMessageHandler(this.chatService, this.userService, this.queueService, this.chatRepository, this.queueRepository),
                new QueueMessageHandler(this.chatService, this.userService, this.queueService, this.chatRepository)
            };
        }
    }
}

using System.Collections.Generic;
using Enqueuer.Messages.MessageHandlers;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Configuration;

namespace Enqueuer.Messages.Factories
{
    /// <inheritdoc/>
    public class MessageHandlersFactory : IMessageHandlersFactory
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;
        private readonly IUserInQueueService userInQueueService;
        private readonly IRepository<UserInQueue> userInQueueRepository;
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="userInQueueRepository">User in queue repository to use.</param>
        /// <param name="botConfiguration">Bot configuration to rely on.</param>
        public MessageHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository,
            IUserInQueueService userInQueueService,
            IRepository<UserInQueue> userInQueueRepository,
            IBotConfiguration botConfiguration)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.queueRepository = queueRepository;
            this.userInQueueService = userInQueueService;
            this.userInQueueRepository = userInQueueRepository;
            this.botConfiguration = botConfiguration;
        }

        /// <inheritdoc/>
        public IEnumerable<IMessageHandler> CreateMessageHandlers()
        {
            return new IMessageHandler[]
            {
                new StartMessageHandler(this.botConfiguration, this.userService),
                new HelpMessageHandler(),
                new CreateQueueMessageHandler(this.chatService, this.userService, this.queueService, this.queueRepository, this.botConfiguration),
                new QueueMessageHandler(this.chatService, this.userService, this.queueService),
                new EnqueueMessageHandler(this.chatService, this.userService, this.queueService, this.userInQueueService, this.userInQueueRepository),
                new RemoveQueueMessageHandler(this.chatService, this.userService, this.queueService),
                new DequeueMessageHandler(this.chatService, this.userService, this.queueService)
            };
        }
    }
}

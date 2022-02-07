using System.Collections.Generic;
using Enqueuer.Bot.Callbacks.CallbackHandlers;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Bot.Factories
{
    /// <inheritdoc/>
    public class CallbackHandlersFactory : ICallbackHandlersFactory
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public CallbackHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public IEnumerable<ICallbackHandler> CreateCallbackHandlers()
        {
            return new ICallbackHandler[]
            {
                new EnqueueMeCallbackHandler(this.chatService, this.userService, this.queueService, this.queueRepository),
            };
        }
    }
}

using System.Collections.Generic;
using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Callbacks.Factories
{
    /// <inheritdoc/>
    public class CallbackHandlersFactory : ICallbackHandlersFactory
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;
        private readonly IRepository<UserInQueue> userInQueueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="userInQueueRepository">User in queue repository to use.</param>
        public CallbackHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IRepository<UserInQueue> userInQueueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.userInQueueRepository = userInQueueRepository;
        }

        /// <inheritdoc/>
        public IEnumerable<ICallbackHandler> CreateCallbackHandlers()
        {
            return new ICallbackHandler[]
            {
                new EnqueueMeCallbackHandler(this.chatService, this.userService, this.queueService, this.userInQueueService, this.userInQueueRepository),
                new ViewChatsCallbackHandler(this.userService)
            };
        }
    }
}

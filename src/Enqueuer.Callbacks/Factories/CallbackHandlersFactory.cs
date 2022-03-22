using System.Collections.Generic;
using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data.DataSerialization;
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
        private readonly IDataSerializer dataSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="dataSerializer"></param>
        public CallbackHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IDataSerializer dataSerializer)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.dataSerializer = dataSerializer;
        }

        /// <inheritdoc/>
        public IEnumerable<ICallbackHandler> CreateCallbackHandlers()
        {
            return new ICallbackHandler[]
            {
                new EnqueueMeCallbackHandler(this.chatService, this.userService, this.queueService, this.userInQueueService),
                new GetChatCallbackHandler(this.chatService, this.dataSerializer),
                new GetQueueCallbackHandler(this.queueService, this.userService, this.dataSerializer),
                new ListChatsCallbackHandler(this.userService, this.dataSerializer),
                new EnqueueCallbackHandler(this.queueService, this.userInQueueService, this.dataSerializer),
                new EnqueueAtCallbackHandler(this.userService, this.queueService, this.userInQueueService, this.dataSerializer),
                new DequeueMeCallbackHandler(this.userService, this.queueService, this.dataSerializer),
                new RemoveQueueCallbackHandler(this.queueService, this.dataSerializer),
            };
        }
    }
}

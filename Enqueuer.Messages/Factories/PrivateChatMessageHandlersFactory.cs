using System.Collections.Generic;
using Enqueuer.Messages.PrivateChatMessageHandlers;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Messages.Factories
{
    /// <inheritdoc/>
    public class PrivateChatMessageHandlersFactory : IPrivateChatMessageHandlersFactory
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateChatMessageHandlersFactory"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        public PrivateChatMessageHandlersFactory(IUserService userService)
        {
            this.userService = userService;
        }

        /// <inheritdoc/>
        public IEnumerable<IPrivateChatMessageHandler> CreateMessageHandlers()
        {
            return new IPrivateChatMessageHandler[]
            {
                new StartPrivateMessageHandler(this.userService)
            };
        }
    }
}

using System.Collections.Generic;
using Enqueuer.Messages.PrivateChatMessageHandlers;

namespace Enqueuer.Messages.Factories
{
    /// <summary>
    /// Creates private chat message handlers to handle incoming messages.
    /// </summary>
    public interface IPrivateChatMessageHandlersFactory
    {
        /// <summary>
        /// Creates <see cref="IEnumerable{T}"/> of private chat message handlers to handle incoming messages.
        /// </summary>
        /// <returns>Message handlers.</returns>
        public IEnumerable<IPrivateChatMessageHandler> CreateMessageHandlers();
    }
}

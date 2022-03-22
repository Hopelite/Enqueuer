using System.Collections.Generic;
using Enqueuer.Messages.MessageHandlers;

namespace Enqueuer.Messages.Factories
{
    /// <summary>
    /// Creates message handlers to handle incoming messages.
    /// </summary>
    public interface IMessageHandlersFactory
    {
        /// <summary>
        /// Creates <see cref="IEnumerable{T}"/> of message handlers to handle incoming messages.
        /// </summary>
        /// <returns>Message handlers.</returns>
        public IEnumerable<IMessageHandler> CreateMessageHandlers();
    }
}

using System.Collections.Generic;
using System.Linq;
using Enqueuer.Bot.Messages.MessageHandlers;

namespace Enqueuer.Bot.Factories
{
    /// <inheritdoc/>
    public class MessageHandlersFactory : IMessageHandlersFactory
    {
        /// <inheritdoc/>
        public IEnumerable<IMessageHandler> CreateMessageHandlers()
        {
            return Enumerable.Empty<IMessageHandler>();
        }
    }
}

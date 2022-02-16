using System;
using Enqueuer.Messages.MessageHandlers;

namespace Enqueuer.Messages.Exceptions
{
    public class MessageHandlerAlreadyInUseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAlreadyInUseException"/> class.
        /// </summary>
        public MessageHandlerAlreadyInUseException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAlreadyInUseException"/> class.
        /// </summary>
        /// <param name="message">Exception description message.</param>
        public MessageHandlerAlreadyInUseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAlreadyInUseException"/> class.
        /// </summary>
        /// <param name="messageHandler">Message handler which command use in exception description.</param>
        public MessageHandlerAlreadyInUseException(IMessageHandler messageHandler)
            : base($"Message handler with command '{messageHandler.Command}' is already in use.")
        {
        }
    }
}

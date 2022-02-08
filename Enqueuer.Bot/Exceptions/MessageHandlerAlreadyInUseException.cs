using System;
using Enqueuer.Bot.Messages.MessageHandlers;

namespace Enqueuer.Bot.Exceptions
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

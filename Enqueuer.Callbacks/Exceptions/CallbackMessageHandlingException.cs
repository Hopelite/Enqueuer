using System;

namespace Enqueuer.Callbacks.Exceptions
{
    /// <summary>
    /// Represents an exception which occurs durin callback handling.
    /// </summary>
    public class CallbackMessageHandlingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMessageHandlingException"/> class.
        /// </summary>
        public CallbackMessageHandlingException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMessageHandlingException"/> class with specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Message to show in exception.</param>
        public CallbackMessageHandlingException(string message)
            : base(message)
        {
        }
    }
}

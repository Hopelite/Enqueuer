using Enqueuer.Bot.Callbacks.CallbackHandlers;
using System.Collections.Generic;

namespace Enqueuer.Bot.Factories
{
    /// <summary>
    /// Creates callback handlers to handle incoming callbacks.
    /// </summary>
    public interface ICallbackHandlersFactory
    {
        /// <summary>
        /// Creates <see cref="IEnumerable{T}"/> of callback handlers to handle incoming callbacks.
        /// </summary>
        /// <returns>Callback handlers.</returns>
        public IEnumerable<ICallbackHandler> CreateCallbackHandlers();
    }
}

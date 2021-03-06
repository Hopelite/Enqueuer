using System.Collections.Generic;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

namespace Enqueuer.Callbacks.Factories
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

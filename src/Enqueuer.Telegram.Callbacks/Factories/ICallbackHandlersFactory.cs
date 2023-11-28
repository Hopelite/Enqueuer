using System.Diagnostics.CodeAnalysis;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;

namespace Enqueuer.Telegram.Callbacks.Factories;

/// <summary>
/// Creates callback handlers to handle incoming callbacks.
/// </summary>
public interface ICallbackHandlersFactory
{
    /// <summary>
    /// Tries to create an appropriate <paramref name="callbackHandler"/> for the <paramref name="callbackContext"/>.
    /// </summary>
    bool TryCreateCallbackHandler(Messaging.Core.Types.Callbacks.CallbackContext callbackContext, [NotNullWhen(returnValue: true)] out ICallbackHandler? callbackHandler);
}

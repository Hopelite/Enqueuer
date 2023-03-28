using System.Diagnostics.CodeAnalysis;
using Enqueuer.Callbacks.CallbackHandlers;

namespace Enqueuer.Callbacks.Factories;

/// <summary>
/// Creates callback handlers to handle incoming callbacks.
/// </summary>
public interface ICallbackHandlersFactory
{
    /// <summary>
    /// Tries to create an appropriate callback handler for the <paramref name="callback"/>.
    /// </summary>
    bool TryCreateCallbackHandler(Callback callback, [NotNullWhen(returnValue: true)] out ICallbackHandler? callbackHandler);
}

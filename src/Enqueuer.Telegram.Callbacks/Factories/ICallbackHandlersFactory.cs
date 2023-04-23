using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;

namespace Enqueuer.Telegram.Callbacks.Factories;

/// <summary>
/// Creates callback handlers to handle incoming callbacks.
/// </summary>
public interface ICallbackHandlersFactory
{
    /// <summary>
    /// Tries to create an appropriate <paramref name="callbackHandler"/> for the <paramref name="callback"/>.
    /// </summary>
    bool TryCreateCallbackHandler(Callback callback, [NotNullWhen(returnValue: true)] out ICallbackHandler? callbackHandler);
}

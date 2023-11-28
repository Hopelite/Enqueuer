using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;

namespace Enqueuer.Telegram.Callbacks;

/// <summary>
/// Distributes callbacks to callback handlers.
/// </summary>
public interface ICallbackDistributor
{
    /// <summary>
    /// Distributes the <paramref name="callbackContext"/> to an appropriate <see cref="ICallbackHandler"/>, if exists.
    /// </summary>
    Task DistributeAsync(CallbackContext callbackContext, CancellationToken cancellationToken);
}

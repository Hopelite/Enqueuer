using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Callbacks;

/// <summary>
/// Distributes callbacks to callback handlers.
/// </summary>
public interface ICallbackDistributor
{
    /// <summary>
    /// Distributes the <paramref name="callbackQuery"/> to an appropriate <see cref="ICallbackHandler"/>, if exists.
    /// </summary>
    Task DistributeAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken);
}

using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks;

/// <summary>
/// Distributes callbacks to callback handlers.
/// </summary>
public interface ICallbackDistributor
{
    /// <summary>
    /// Distributes the <paramref name="callbackQuery"/> to an appropriate <see cref="ICallbackHandler"/>, if exists.
    /// </summary>
    Task DistributeCallbackAsync(CallbackQuery callbackQuery);
}

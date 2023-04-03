using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks.CallbackHandlers;

/// <summary>
/// Handles the incoming <see cref="CallbackQuery"/>.
/// </summary>
public interface ICallbackHandler
{
    /// <summary>
    /// Handles the incoming <paramref name="callback"/>.
    /// </summary>
    Task HandleAsync(Callback callback, CancellationToken cancellationToken);
}

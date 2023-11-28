using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

/// <summary>
/// Handles the incoming <see cref="CallbackQuery"/>.
/// </summary>
public interface ICallbackHandler
{
    /// <summary>
    /// Handles the incoming <paramref name="callbackContext"/>.
    /// </summary>
    Task HandleAsync(CallbackContext callbackContext, CancellationToken cancellationToken);
}

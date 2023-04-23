using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

/// <summary>
/// Handles the incoming <see cref="Message"/>.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Handles the incoming <paramref name="message"/>.
    /// </summary>
    Task HandleAsync(Message message, CancellationToken cancellationToken);
}

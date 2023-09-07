using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Messages;
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
    Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken);
}

using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Core.Types.Messages;
using Enqueuer.Telegram.Messages.MessageHandlers;

namespace Enqueuer.Telegram.Messages;

/// <summary>
/// Distributes messages to message handlers.
/// </summary>
public interface IMessageDistributor
{
    /// <summary>
    /// Distributes the <paramref name="messageContext"/> to an appropriate <see cref="IMessageHandler"/>, if exists.
    /// </summary>
    Task DistributeAsync(MessageContext messageContext, CancellationToken cancellationToken);
}

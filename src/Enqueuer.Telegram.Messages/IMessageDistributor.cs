using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Messages.MessageHandlers;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Messages;

/// <summary>
/// Distributes messages to message handlers.
/// </summary>
public interface IMessageDistributor
{
    /// <summary>
    /// Distributes the <paramref name="message"/> to an appropriate <see cref="IMessageHandler"/>, if exists.
    /// </summary>
    Task DistributeAsync(Message message, CancellationToken cancellationToken);
}

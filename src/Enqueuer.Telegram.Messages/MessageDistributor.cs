using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Messages.Factories;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Messages;

public class MessageDistributor : IMessageDistributor
{
    private readonly IMessageHandlersFactory _messageHandlersFactory;

    public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
    {
        _messageHandlersFactory = messageHandlersFactory;
    }

    public async Task DistributeAsync(Message message, CancellationToken cancellationToken)
    {
        if (_messageHandlersFactory.TryCreateMessageHandler(message, out var handler))
        {
            await handler.HandleAsync(message, cancellationToken);
        }
    }
}

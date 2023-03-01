using System.Threading.Tasks;
using Enqueuer.Messages.Factories;
using Telegram.Bot.Types;

namespace Enqueuer.Messages;

public class MessageDistributor : IMessageDistributor
{
    private readonly IMessageHandlersFactory _messageHandlersFactory;

    public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
    {
        _messageHandlersFactory = messageHandlersFactory;
    }

    public async Task DistributeAsync(Message message)
    {
        if (_messageHandlersFactory.TryCreateMessageHandler(message, out var handler))
        {
            await handler.HandleAsync(message);
        }
    }
}

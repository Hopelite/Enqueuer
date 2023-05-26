﻿using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Core.Types.Messages;
using Enqueuer.Telegram.Messages.Factories;

namespace Enqueuer.Telegram.Messages;

public class MessageDistributor : IMessageDistributor
{
    private readonly IMessageHandlersFactory _messageHandlersFactory;

    public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
    {
        _messageHandlersFactory = messageHandlersFactory;
    }

    public async Task DistributeAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (_messageHandlersFactory.TryCreateMessageHandler(messageContext, out var handler))
        {
            await Task.Delay(1000);
            //await handler.HandleAsync(messageContext, cancellationToken);
        }
    }
}

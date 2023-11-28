﻿using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Telegram.Messages.Factories;

namespace Enqueuer.Telegram.Messages;

public class MessageDistributor : IMessageDistributor
{
    private readonly IMessageHandlersFactory _messageHandlersFactory;

    public MessageDistributor(IMessageHandlersFactory messageHandlersFactory)
    {
        _messageHandlersFactory = messageHandlersFactory;
    }

    public Task DistributeAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (_messageHandlersFactory.TryCreateMessageHandler(messageContext, out var handler))
        {
            return handler.HandleAsync(messageContext, cancellationToken);
        }

        return Task.CompletedTask;
    }
}

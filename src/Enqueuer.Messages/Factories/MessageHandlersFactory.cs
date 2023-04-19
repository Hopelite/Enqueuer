using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Data.Constants;
using Enqueuer.Messages.Extensions;
using Enqueuer.Messages.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.Factories
{
    public class MessageHandlersFactory : IMessageHandlersFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHandlersFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool TryCreateMessageHandler(Message message, [NotNullWhen(returnValue: true)] out IMessageHandler? messageHandler)
        {
            messageHandler = null;
            if (message == null || string.IsNullOrWhiteSpace(message.Text) || !message.Text.TryGetCommand(out var command))
            {
                return false;
            }

            return TryCreateMessageHandler(command, out messageHandler);
        }

        private bool TryCreateMessageHandler(string command, out IMessageHandler? messageHandler)
        {
            messageHandler = command switch
            {
                MessageConstants.StartCommand => _serviceProvider.GetRequiredService<StartMessageHandler>(),
                MessageConstants.HelpCommand => _serviceProvider.GetRequiredService<HelpMessageHandler>(),
                MessageConstants.QueueCommand => _serviceProvider.GetRequiredService<QueueMessageHandler>(),
                MessageConstants.EnqueueCommand => _serviceProvider.GetRequiredService<EnqueueMessageHandler>(),
                MessageConstants.DequeueCommand => _serviceProvider.GetRequiredService<DequeueMessageHandler>(),
                MessageConstants.CreateQueueCommand => _serviceProvider.GetRequiredService<CreateQueueMessageHandler>(),
                MessageConstants.RemoveQueueCommand => _serviceProvider.GetRequiredService<RemoveQueueMessageHandler>(),
                _ => null
            };

            return messageHandler != null;
        }
    }
}

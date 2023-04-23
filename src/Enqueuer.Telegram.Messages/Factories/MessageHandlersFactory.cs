using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Messages.Extensions;
using Enqueuer.Telegram.Messages.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Messages.Factories
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
                MessageCommands.StartCommand => _serviceProvider.GetRequiredService<StartMessageHandler>(),
                MessageCommands.HelpCommand => _serviceProvider.GetRequiredService<HelpMessageHandler>(),
                MessageCommands.QueueCommand => _serviceProvider.GetRequiredService<QueueMessageHandler>(),
                MessageCommands.EnqueueCommand => _serviceProvider.GetRequiredService<EnqueueMessageHandler>(),
                MessageCommands.DequeueCommand => _serviceProvider.GetRequiredService<DequeueMessageHandler>(),
                MessageCommands.CreateQueueCommand => _serviceProvider.GetRequiredService<CreateQueueMessageHandler>(),
                MessageCommands.RemoveQueueCommand => _serviceProvider.GetRequiredService<RemoveQueueMessageHandler>(),
                _ => null
            };

            return messageHandler != null;
        }
    }
}

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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MessageHandlersFactory(IServiceScopeFactory serviceProvider)
        {
            _serviceScopeFactory = serviceProvider;
        }

        public bool TryCreateMessageHandler(Message message, [NotNullWhen(returnValue: true)] out IMessageHandler? messageHandler)
        {
            messageHandler = null;
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
            {
                return false;
            }

            var command = message.Text.SplitToWords()[0];
            return TryCreateMessageHandler(command, out messageHandler);
        }

        private bool TryCreateMessageHandler(string command, out IMessageHandler? messageHandler)
        {
            messageHandler = command switch
            {
                MessageConstants.StartCommand => new StartMessageHandler(_serviceScopeFactory),
                MessageConstants.HelpCommand => new HelpMessageHandler(_serviceScopeFactory),
                MessageConstants.QueueCommand => new QueueMessageHandler(_serviceScopeFactory),
                MessageConstants.EnqueueCommand => new EnqueueMessageHandler(_serviceScopeFactory),
                MessageConstants.DequeueCommand => new DequeueMessageHandler(_serviceScopeFactory),
                MessageConstants.CreateQueueCommand => new CreateQueueMessageHandler(_serviceScopeFactory),
                MessageConstants.RemoveQueueCommand => new RemoveQueueMessageHandler(_serviceScopeFactory),
                _ => null
            };

            return messageHandler != null;
        }
    }
}

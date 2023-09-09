using System.Diagnostics.CodeAnalysis;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Telegram.Messages.MessageHandlers;

namespace Enqueuer.Telegram.Messages.Factories;

/// <summary>
/// Creates message handlers to handle incoming messages.
/// </summary>
public interface IMessageHandlersFactory
{
    /// <summary>
    /// Tries to create an appropriate message handler for the <paramref name="messageContext"/>.
    /// </summary>
    bool TryCreateMessageHandler(MessageContext messageContext, [NotNullWhen(returnValue: true)] out IMessageHandler? messageHandler);
}

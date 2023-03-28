using System.Diagnostics.CodeAnalysis;
using Enqueuer.Messages.MessageHandlers;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.Factories;

/// <summary>
/// Creates message handlers to handle incoming messages.
/// </summary>
public interface IMessageHandlersFactory
{
    /// <summary>
    /// Tries to create an appropriate message handler for the <paramref name="message"/>.
    /// </summary>
    bool TryCreateMessageHandler(Message message, [NotNullWhen(returnValue: true)] out IMessageHandler? messageHandler);
}

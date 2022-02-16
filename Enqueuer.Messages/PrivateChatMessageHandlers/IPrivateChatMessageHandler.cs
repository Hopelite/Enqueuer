using Enqueuer.Messages.MessageHandlers;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.PrivateChatMessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> from private chat.
    /// </summary>
    public interface IPrivateChatMessageHandler : IMessageHandler
    {
    }
}

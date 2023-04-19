using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core;
using Enqueuer.Core.Constants;
using Enqueuer.Core.Serialization;
using Enqueuer.Core.TextProviders;
using Enqueuer.Persistence.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers;

public abstract class MessageHandlerWithEnqueueMeButton : IMessageHandler
{
    protected readonly IMessageProvider MessageProvider;
    protected readonly ICallbackDataSerializer DataSerializer;

    protected MessageHandlerWithEnqueueMeButton(IMessageProvider messageProvider, ICallbackDataSerializer dataSerializer)
    {
        MessageProvider = messageProvider;
        DataSerializer = dataSerializer;
    }

    public abstract Task HandleAsync(Message message, CancellationToken cancellationToken);

    protected InlineKeyboardButton GetEnqueueMeButton(Group group, int queueId)
    {
        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.EnqueueMeCommand,
            TargetChatId = group.Id,
            QueueData = new QueueData()
            {
                QueueId = queueId,
            },
        };

        var serializedButtonData = DataSerializer.Serialize(callbackButtonData);
        return InlineKeyboardButton.WithCallbackData(
            MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_EnqueueMeButton),
            serializedButtonData);
    }
}

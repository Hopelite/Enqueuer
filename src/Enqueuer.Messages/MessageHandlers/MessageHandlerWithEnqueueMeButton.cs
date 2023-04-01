using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers;

public abstract class MessageHandlerWithEnqueueMeButton : IMessageHandler
{
    protected readonly IMessageProvider MessageProvider;
    protected readonly IDataSerializer DataSerializer;

    protected MessageHandlerWithEnqueueMeButton(IMessageProvider messageProvider, IDataSerializer dataSerializer)
    {
        MessageProvider = messageProvider;
        DataSerializer = dataSerializer;
    }

    public abstract Task HandleAsync(Message message);

    protected InlineKeyboardButton GetEnqueueMeButton(Group group, Queue queue)
    {
        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.EnqueueMeCommand,
            ChatId = group.Id,
            QueueData = new QueueData()
            {
                QueueId = queue.Id,
            },
        };

        var serializedButtonData = DataSerializer.Serialize(callbackButtonData);
        return InlineKeyboardButton.WithCallbackData(
            MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_EnqueueMeButton),
            serializedButtonData);
    }
}

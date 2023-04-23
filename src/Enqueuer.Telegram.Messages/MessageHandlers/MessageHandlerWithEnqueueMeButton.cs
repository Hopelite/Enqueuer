﻿using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public abstract class MessageHandlerWithEnqueueMeButton : IMessageHandler
{
    protected readonly ILocalizationProvider LocalizationProvider;
    protected readonly ICallbackDataSerializer DataSerializer;

    protected MessageHandlerWithEnqueueMeButton(ILocalizationProvider localizationProvider, ICallbackDataSerializer dataSerializer)
    {
        LocalizationProvider = localizationProvider;
        DataSerializer = dataSerializer;
    }

    public abstract Task HandleAsync(Message message, CancellationToken cancellationToken);

    protected InlineKeyboardButton GetEnqueueMeButton(Group group, int queueId)
    {
        var callbackButtonData = new CallbackData()
        {
            Command = CallbackCommands.EnqueueMeCommand,
            TargetChatId = group.Id,
            QueueData = new QueueData()
            {
                QueueId = queueId,
            },
        };

        var serializedButtonData = DataSerializer.Serialize(callbackButtonData);
        return InlineKeyboardButton.WithCallbackData(
            LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_EnqueueMe_Button, MessageParameters.None),
            serializedButtonData);
    }
}

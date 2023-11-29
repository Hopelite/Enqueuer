using System;
using System.Collections.Generic;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Persistence.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.Helpers;

public class ReplyMarkupBuilder
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly Queue<IEnumerable<InlineKeyboardButton>> _markup;
    private readonly Queue<InlineKeyboardButton> _row;

    private ReplyMarkupBuilder(ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : this(dataSerializer, localizationProvider, new Queue<IEnumerable<InlineKeyboardButton>>())
    {
    }

    private ReplyMarkupBuilder(ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, Queue<IEnumerable<InlineKeyboardButton>> markup)
    {
        _dataSerializer = dataSerializer;
        _localizationProvider = localizationProvider;
        _markup = markup;
        _row = new Queue<InlineKeyboardButton>();
    }

    public static ReplyMarkupBuilder Create(ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        => new ReplyMarkupBuilder(dataSerializer, localizationProvider);

    public ReplyMarkupBuilder FromNewRow()
    {
        _markup.Enqueue(_row);
        return new ReplyMarkupBuilder(_dataSerializer, _localizationProvider, _markup);
    }

    public ReplyMarkupBuilder ForEach()
    {
        return this;
    }

    public InlineKeyboardMarkup Build()
    {
        _markup.Enqueue(_row);
        return new InlineKeyboardMarkup(_markup);
    }

    /// <summary>
    /// Adds the refresh button with the <paramref name="callbackData"/>.
    /// </summary>
    public ReplyMarkupBuilder WithRefreshButton(CallbackData callbackData)
    {
        var serializedCallbackData = _dataSerializer.Serialize(callbackData);
        var button = InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(CallbackMessageKeys.Callback_RefreshMessage_Button, MessageParameters.None), serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    /// <summary>
    /// Creates a button for pagination.
    /// </summary>
    public ReplyMarkupBuilder WithAnotherPageButton(CallbackData previousCallbackData, int page, string buttonText)
    {
        var newCallbackData = new CallbackData
        {
            Command = previousCallbackData.Command,
            Page = page,
            UserAgreement = previousCallbackData.UserAgreement,
            QueueData = previousCallbackData.QueueData,
            TargetChatId = previousCallbackData.TargetChatId,
            TargetUserId = previousCallbackData.TargetUserId,
        };

        var serializedCallbackData = _dataSerializer.Serialize(newCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    /// <summary>
    /// Adds the remove queue button.
    /// </summary>
    /// <param name="isAgreed">Value indicating whether user is agreed to delete queue or not. Null, if user wasn't prompted to delete queue yet.</param>
    public ReplyMarkupBuilder WithRemoveQueueButton(string buttonText, CallbackData callbackData, bool? isAgreed = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.RemoveQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            UserAgreement = isAgreed,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData.QueueId, // TODO: verify that not null
            }
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    /// <summary>
    /// Adds the return to chat button.
    /// </summary>
    public ReplyMarkupBuilder WithReturnToChatButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetChatCommand,
            TargetChatId = callbackData.TargetChatId,
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    /// <summary>
    /// Adds the return to queue button.
    /// </summary>
    public ReplyMarkupBuilder WithReturnToQueueButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = callbackData.QueueData,
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithQueueRelatedButton(string buttonText, string command, CallbackData callbackData, int queueId)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = command,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = queueId,
            }
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithDequeueButton(string buttonText, CallbackData callbackData, bool? isAgreed = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.DequeueMeCommand,
            TargetChatId = callbackData.TargetChatId,
            UserAgreement = isAgreed,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData!.QueueId,
            }
        };

        var serializedCallbackData =_dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithEnqueueAtButton(CallbackData callbackData, string? buttonText = null, int? position = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.EnqueueAtCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData!.QueueId,
                Position = position,
            },
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData($"{buttonText ?? position?.ToString() ?? throw new ArgumentNullException(nameof(buttonText))}", serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithDynamicQueueButton(string command, CallbackData callbackData, Queue queue)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = command,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = queue.Id,
            }
        };

        var buttonText = queue.IsDynamic
            ? _localizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_MakeQueueStatic_Button, MessageParameters.None)
            : _localizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_MakeQueueDynamic_Button, MessageParameters.None);

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithExchangeRequestButton(QueueMember queueMember, int sourcePosition)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.ExchangePositionsCommand,
            TargetUserId = queueMember.UserId,
            QueueData = new QueueData()
            {
                QueueId = queueMember.QueueId,
                Position = sourcePosition,
            },
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData($"{queueMember.Position}) {queueMember.User.FullName}", serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithExchangeRequestResponseButton(QueueMember queueMember, string buttonText, bool userAgreement)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.ExchangePositionsCommand,
            TargetUserId = queueMember.UserId,
            QueueData = new QueueData()
            {
                QueueId = queueMember.QueueId,
                Position = queueMember.Position,
            },
            UserAgreement = userAgreement
        };

        var serializedCallbackData = _dataSerializer.Serialize(buttonCallbackData);
        var button = InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithOpenChatButton(Group group)
    {
        var callbackData = new CallbackData()
        {
            Command = CallbackCommands.GetChatCommand,
            TargetChatId = group.Id,
        };

        var serializedCallbackData = _dataSerializer.Serialize(callbackData);
        var button = InlineKeyboardButton.WithCallbackData(group.Title, serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithOpenQueueButton(long chatId, Queue queue)
    {
        var newCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetQueueCommand,
            TargetChatId = chatId,
            QueueData = new QueueData()
            {
                QueueId = queue.Id,
            },
        };

        var serializedCallbackData = _dataSerializer.Serialize(newCallbackData);
        var button = InlineKeyboardButton.WithCallbackData($"'{queue.Name}'", serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }

    public ReplyMarkupBuilder WithReturnToChatListButton()
    {
        var callbackData = new CallbackData()
        {
            Command = CallbackCommands.ListChatsCommand,
        };

        var serializedCallbackData = _dataSerializer.Serialize(callbackData);
        var button = InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
        _row.Enqueue(button);
        return this;
    }
}

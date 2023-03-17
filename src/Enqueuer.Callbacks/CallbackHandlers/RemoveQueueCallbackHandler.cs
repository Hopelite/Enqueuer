using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class RemoveQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<DequeueMeCallbackHandler> _logger;

    public RemoveQueueCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _userService = userService;
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html);
        }

        return HandleAsyncInternal(callback);
    }

    private async Task HandleAsyncInternal(Callback callback)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: false, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                "This queue has already been deleted.",
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task<Message> HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        if (HasUserAgreement(callback.CallbackData!))
        {
            return await HandleCallbackWithUserAgreementAsync(callback, queue);
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetRemoveQueueButton("Yes, delete it", callback.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) }
        };

        return await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            $"Do you really want to delete the <b>'{queue.Name}'</b> queue? This action cannot be undone.",
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task<Message> HandleCallbackWithUserAgreementAsync(Callback callback, Queue queue)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        if (callback.CallbackData.QueueData.IsUserAgreed!.Value)
        {
            var userId = callback.From.Id;
            var chat = queue.Group;
            if (!queue.IsQueueCreator(userId) && !await TelegramBotClient.IsChatAdmin(userId, chat.Id))
            {
                return await TelegramBotClient.EditMessageTextAsync(
                    callback.Message.Chat,
                    callback.Message.MessageId,
                    $"Unable to delete <b>'{queue.Name}'</b> queue: you are not queue creator or the chat admin.",
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callback.CallbackData));
            }

            await _queueService.DeleteQueueAsync(queue, CancellationToken.None);

            await TelegramBotClient.SendTextMessageAsync(
                chat.Id,
                $"{user.FullName} deleted <b>'{queue.Name}'</b> queue. I shall miss it.",
                ParseMode.Html);

            return await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                $"Successfully deleted the <b>'{queue.Name}'</b> queue.",
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callback.CallbackData));
        }

        throw new CallbackMessageHandlingException("False 'IsUserAgreed' value passed to message handler.");
    }

    private static bool HasUserAgreement(CallbackData callbackData)
    {
        return callbackData.QueueData.IsUserAgreed.HasValue;
    }
}

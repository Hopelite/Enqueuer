using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class RemoveQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
    {
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public RemoveQueueCallbackHandler(IQueueService queueService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.RemoveQueueCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = this.queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has already been deleted.",
                        replyMarkup: this.GetReturnToChatButton(callbackData));
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, callbackData, queue);
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData, Queue queue)
        {
            if (HasUserAgreement(callbackData))
            {
                return await this.HandleCallbackWithUserAgreementAsync(botClient, callbackQuery, callbackData, queue);
            }

            var replyMarkup = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[] { this.GetRemoveQueueButton("Yes, delete it", callbackData, true) },
                new InlineKeyboardButton[] { this.GetReturnToQueueButton(callbackData) }
            };

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Do you really want to delete the <b>'{queue.Name}'</b> queue? This action cannot be undone.",
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private async Task<Message> HandleCallbackWithUserAgreementAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData, Queue queue)
        {
            if (callbackData.QueueData.IsUserAgreed.Value)
            {
                var userId = callbackQuery.From.Id;
                var chat = queue.Chat;
                if (!queue.IsQueueCreator(userId) && !await botClient.IsChatAdmin(userId, chat.ChatId))
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"Unable to delete <b>'{queue.Name}'</b> queue: you are not queue creator or the chat admin.",
                        ParseMode.Html,
                        replyMarkup: this.GetReturnToQueueButton(callbackData));
                }

                await this.queueService.DeleteQueueAsync(queue);

                await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName + ' ' ?? string.Empty}deleted <b>'{queue.Name}'</b> queue. I shall miss it.",
                    ParseMode.Html);

                return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    $"Successfully deleted the <b>'{queue.Name}'</b> queue.",
                    ParseMode.Html,
                    replyMarkup: this.GetReturnToChatButton(callbackData));
            }

            throw new CallbackMessageHandlingException("False 'IsUserAgreed' value passed to message handler.");
        }

        private bool HasUserAgreement(CallbackData callbackData)
        {
            return callbackData.QueueData.IsUserAgreed.HasValue;
        }
    }
}

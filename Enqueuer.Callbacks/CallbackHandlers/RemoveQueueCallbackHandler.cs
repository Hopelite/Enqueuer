using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class RemoveQueueCallbackHandler : ICallbackHandler
    {
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        public RemoveQueueCallbackHandler(IQueueService queueService)
        {
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public string Command => "/removequeue";

        /// <inheritdoc/>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data.SplitToWords();
            if (int.TryParse(callbackData[1], out var queueId))
            {
                if (long.TryParse(callbackData[2], out var chatId))
                {
                    var queue = this.queueService.GetQueueById(queueId);
                    if (queue is null)
                    {
                        return await botClient.EditMessageTextAsync(
                            callbackQuery.Message.Chat,
                            callbackQuery.Message.MessageId,
                            "This queue has already been deleted.",
                            replyMarkup: GetReturnButton(chatId));
                    }

                    return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, callbackData, queue, chatId);
                }

                throw new CallbackMessageHandlingException("Invalid chat ID passed to message handler.");
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] callbackData, Queue queue, long chatId)
        {
            if (HasUserAgreement(callbackData))
            {
                return await this.HandleCallbackWithUserAgreementAsync(botClient, callbackQuery, callbackData, queue, chatId);
            }

            var replyMarkup = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Yes, delete it", $"/removequeue {queue.Id} {chatId} true") },
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Return", $"/getqueue {queue.Id} {chatId}") }
            };

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Do you really want to delete the <b>'{queue.Name}'</b> queue? This action cannot be undone.",
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private async Task<Message> HandleCallbackWithUserAgreementAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] callbackData, Queue queue, long chatId)
        {
            if (bool.TryParse(callbackData[3], out bool isAgreed))
            {
                if (isAgreed)
                {
                    var chat = queue.Chat;
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
                        replyMarkup: GetReturnButton(chatId));
                }
            }

            throw new CallbackMessageHandlingException("Invalid user agreement value passed to message handler.");
        }

        private static InlineKeyboardButton GetReturnButton(long chatId)
        {
            return InlineKeyboardButton.WithCallbackData("Return", $"/getchat {chatId}");
        }

        private bool HasUserAgreement(string[] callbackData)
        {
            return callbackData.Length == 4;
        }
    }
}

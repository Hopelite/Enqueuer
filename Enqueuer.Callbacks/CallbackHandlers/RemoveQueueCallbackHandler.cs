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
        private static readonly InlineKeyboardButton ReturnButton = InlineKeyboardButton.WithCallbackData("Return", "/viewchats");
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
                var queue = this.queueService.GetQueueById(queueId);
                if (queue is null)
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has already been deleted.",
                        replyMarkup: ReturnButton);
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, callbackData, queue);
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] callbackData, Queue queue)
        {
            if (HasUserAgreement(callbackData))
            {
                return await this.HandleCallbackWithUserAgreementAsync(botClient, callbackQuery, callbackData, queue);
            }

            var replyMarkup = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Yes, delete it", $"/removequeue {queue.Id} true") },
                new InlineKeyboardButton[] { ReturnButton }
            };

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Do you really want to delete the <b>'{queue.Name}'</b> queue? This action cannot be undone.",
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private async Task<Message> HandleCallbackWithUserAgreementAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] callbackData, Queue queue)
        {
            if (bool.TryParse(callbackData[2], out bool isAgreed))
            {
                if (isAgreed)
                {
                    await this.queueService.DeleteQueueAsync(queue);

                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"Successfully deleted the <b>'{queue.Name}'</b> queue.",
                        ParseMode.Html,
                        replyMarkup: ReturnButton);
                }
            }

            throw new CallbackMessageHandlingException("Invalid user agreement value passed to message handler.");
        }

        private bool HasUserAgreement(string[] callbackData)
        {
            return callbackData.Length == 3;
        }
    }
}

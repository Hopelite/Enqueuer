using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Extensions;
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
    public class EnqueueAtCallbackHandler : ICallbackHandler
    {
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        public EnqueueAtCallbackHandler(
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService)
        {
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
        }

        /// <inheritdoc/>
        public string Command => "/enqueueat";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/enqueueat' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
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
                            "This queue has been deleted.",
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Return", $"/getchat {chatId}"));
                    }

                    return await HandleCallbackWithExistionQueueAsync(botClient, callbackQuery, callbackData, queue, chatId);
                }

                throw new CallbackMessageHandlingException("Invalid chat ID passed to message handler.");
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistionQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] callbackData, Queue queue, long chatId)
        {
            var user = await this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
            if (user.IsParticipatingIn(queue))
            {
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"You're already participating in queue '<b>{queue.Name}</b>'. To change your position, please, dequeue yourself first.",
                        ParseMode.Html,
                        replyMarkup: GetReturnButton(queue.Id, chatId));
            }

            var (message, position) = HasSpecifiedPosition(callbackData)
                ? this.HandleCallbackWithSpecifiedPosition(callbackData, queue)
                : this.HandleCallbackWithoutPositionProvided(queue);

            if (position.HasValue)
            {
                await this.userInQueueService.AddUserToQueue(user, queue, position.Value);
            }

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                message,
                ParseMode.Html,
                replyMarkup: GetReturnButton(queue.Id, chatId));
        }

        private static InlineKeyboardButton GetReturnButton(int queueId, long chatId)
        {
            return InlineKeyboardButton.WithCallbackData("Return", $"/getqueue {queueId} {chatId}");
        }

        private (string message, int? position) HandleCallbackWithSpecifiedPosition(string[] callbackData, Queue queue)
        {
            if (int.TryParse(callbackData[3], out var position))
            {
                if (this.userInQueueService.IsPositionReserved(queue, position))
                {
                    var notAvailableMessage = $"Position '<b>{position}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.";
                    return (notAvailableMessage, null);
                }

                var message = $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{position}</b>!";
                return (message, position);
            }

            throw new CallbackMessageHandlingException("Invalid user position passed to message handler.");
        }

        private (string message, int position) HandleCallbackWithoutPositionProvided(Queue queue)
        {
            var firstPositionAvailable = this.userInQueueService.GetFirstAvailablePosition(queue);
            return ($"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{firstPositionAvailable}</b>!", firstPositionAvailable);
        }

        private bool HasSpecifiedPosition(string[] callbackData)
        {
            return callbackData.Length == 4;
        }
    }
}

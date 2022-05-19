using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueAtCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
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
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public EnqueueAtCallbackHandler(
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.EnqueueAtCommand;

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
                        "This queue has been deleted.",
                        replyMarkup: this.GetReturnToChatButton(callbackData));
                }

                return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = await this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
            if (user.IsParticipatingIn(queue))
            {
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"You're already participating in queue '<b>{queue.Name}</b>'. To change your position, please, dequeue yourself first.",
                        ParseMode.Html,
                        replyMarkup: this.GetReturnToQueueButton(callbackData));
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
                replyMarkup: this.GetReturnToQueueButton(callbackData));
        }

        private (string message, int? position) HandleCallbackWithSpecifiedPosition(CallbackData callbackData, Queue queue)
        {
            var position = callbackData.QueueData.Position.Value;
            if (this.userInQueueService.IsPositionReserved(queue, position))
            {
                var notAvailableMessage = $"Position '<b>{position}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.";
                return (notAvailableMessage, null);
            }

            var message = $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{position}</b>!";
            return (message, position);
        }

        private (string message, int position) HandleCallbackWithoutPositionProvided(Queue queue)
        {
            var firstPositionAvailable = this.userInQueueService.GetFirstAvailablePosition(queue);
            return ($"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{firstPositionAvailable}</b>!", firstPositionAvailable);
        }

        private static bool HasSpecifiedPosition(CallbackData callbackData)
        {
            return callbackData.QueueData.Position.HasValue;
        }
    }
}

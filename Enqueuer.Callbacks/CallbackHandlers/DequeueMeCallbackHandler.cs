using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class DequeueMeCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
    {
        private readonly IUserService userService;
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DequeueMeCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public DequeueMeCallbackHandler(IUserService userService, IQueueService queueService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.userService = userService;
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.DequeueMeCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = this.queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    var returnButton = this.GetReturnToChatButton(callbackData);
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: returnButton);
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = this.userService.GetUserByUserId(callbackQuery.From.Id);
            string responseMessage;
            if (user.IsParticipatingIn(queue))
            {
                await this.queueService.RemoveUserAsync(queue, user);
                responseMessage = $"Successfully removed from the '<b>{queue.Name}</b>' queue!";
            }
            else
            {
                responseMessage = $"You've already dequeued from the '<b>{queue.Name}</b>' queue.";
            }

            var returnButton = this.GetReturnToQueueButton(callbackData);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                responseMessage,
                ParseMode.Html,
                replyMarkup: returnButton);
        }
    }
}

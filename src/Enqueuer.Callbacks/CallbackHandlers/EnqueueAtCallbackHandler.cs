﻿using System.Threading.Tasks;
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
    public class EnqueueAtCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
    {
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly IUserInQueueService _userInQueueService;

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
            _userService = userService;
            _queueService = queueService;
            _userInQueueService = userInQueueService;
        }

        public override string Command => CallbackConstants.EnqueueAtCommand;

        protected override async Task<Message> HandleCallbackAsyncImplementation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = _queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: GetReturnToChatButton(callbackData));
                }

                return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = await _userService.GetOrCreateUserAsync(callbackQuery.From);
            if (user.IsParticipatingIn(queue))
            {
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"You're already participating in queue '<b>{queue.Name}</b>'. To change your position, please, dequeue yourself first.",
                        ParseMode.Html,
                        replyMarkup: GetReturnToQueueButton(callbackData));
            }

            if (HasSpecifiedPosition(callbackData) && queue.IsDynamic)
            {
                return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    $"Queue '<b>{queue.Name}</b>' is now dynamic. Please, return and press the 'First available' button.",
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callbackData));
            }

            var (message, position) = HasSpecifiedPosition(callbackData)
                ? HandleCallbackWithSpecifiedPosition(callbackData, queue)
                : HandleCallbackWithoutPositionProvided(queue);

            if (position.HasValue)
            {
                await _userInQueueService.AddUserToQueueAsync(user, queue, position.Value);
            }

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                message,
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callbackData));
        }

        private (string message, int? position) HandleCallbackWithSpecifiedPosition(CallbackData callbackData, Queue queue)
        {
            var position = callbackData.QueueData.Position.Value;
            if (_userInQueueService.IsPositionReserved(queue, position))
            {
                var notAvailableMessage = $"Position '<b>{position}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.";
                return (notAvailableMessage, null);
            }

            var message = $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{position}</b>!";
            return (message, position);
        }

        private (string message, int position) HandleCallbackWithoutPositionProvided(Queue queue)
        {
            var firstPositionAvailable = _userInQueueService.GetFirstAvailablePosition(queue);
            return ($"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{firstPositionAvailable}</b>!", firstPositionAvailable);
        }

        private static bool HasSpecifiedPosition(CallbackData callbackData)
        {
            return callbackData.QueueData.Position.HasValue;
        }
    }
}
﻿using System.Threading.Tasks;
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
    public class DequeueMeCallbackHandler : ICallbackHandler
    {
        private static readonly InlineKeyboardButton ReturnButton = InlineKeyboardButton.WithCallbackData("Return", "/viewchats");
        private readonly IUserService userService;
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DequeueMeCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        public DequeueMeCallbackHandler(IUserService userService, IQueueService queueService)
        {
            this.userService = userService;
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public string Command => "/dequeueme";

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
                        "This queue has been deleted.",
                        replyMarkup: ReturnButton);
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue);
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue)
        {
            var user = this.userService.GetUserByUserId(callbackQuery.From.Id);
            if (user.IsParticipatingIn(queue))
            {
                await this.queueService.RemoveUserAsync(queue, user);
                return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    $"Successfully removed from the '<b>{queue.Name}</b>' queue!",
                    ParseMode.Html,
                    replyMarkup: ReturnButton);
            }

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"You've already dequeued from the '<b>{queue.Name}</b>' queue.",
                replyMarkup: ReturnButton);
        }
    }
}

using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IRepository<UserInQueue> userInQueueRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="userInQueueRepository">User in queue repository to use.</param>
        /// <param name="logger">Logger to log errors.</param>
        public EnqueueAtCallbackHandler(
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IRepository<UserInQueue> userInQueueRepository,
            ILogger logger)
        {
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.userInQueueRepository = userInQueueRepository;
            this.logger = logger;
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
            if (int.TryParse(callbackData[^1], out var queueId))
            {
                var queue = this.queueService.GetQueueById(queueId);
                if (queue is null)
                {
                    var returnButton = InlineKeyboardButton.WithCallbackData("Return", $"/viewchats");
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: returnButton);
                }

                if (HasSpecifiedPosition(callbackData))
                {
                    if (int.TryParse(callbackData[1], out var position))
                    {
                        if (this.userInQueueService.IsPositionReserved(queue, position))
                        {
                            var returnButton = InlineKeyboardButton.WithCallbackData("Return", $"/viewchats");
                            return await botClient.EditMessageTextAsync(
                                    callbackQuery.Message.Chat,
                                    callbackQuery.Message.MessageId,
                                    $"Position '<b>{position}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.",
                                    ParseMode.Html,
                                    replyMarkup: returnButton);
                        }

                        var user = this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
                        var userInQueue = new UserInQueue()
                        {
                            Position = position,
                            UserId = user.Id,
                            QueueId = queue.Id,
                        };

                        await this.userInQueueRepository.AddAsync(userInQueue);
                        var replyMarkup = InlineKeyboardButton.WithCallbackData("Return", $"/viewchats");
                        return await botClient.EditMessageTextAsync(
                            callbackQuery.Message.Chat,
                            callbackQuery.Message.MessageId,
                            $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{position}</b>!",
                            ParseMode.Html,
                            replyMarkup: replyMarkup);
                    }

                    this.logger.LogError("Invalid user position passed to message handler.");
                    return null;
                }
                else
                {
                    var firstPositionAvailable = this.userInQueueService.GetFirstAvailablePosition(queue);
                    // Error here
                    var user = this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
                    var userInQueue = new UserInQueue()
                    {
                        Position = firstPositionAvailable,
                        UserId = user.Id,
                        QueueId = queue.Id,
                    };

                    await this.userInQueueRepository.AddAsync(userInQueue);
                    var replyMarkup = InlineKeyboardButton.WithCallbackData("Return", $"/viewchats");
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{firstPositionAvailable}</b>!",
                        ParseMode.Html,
                        replyMarkup: replyMarkup);
                }
            }

            this.logger.LogError("Invalid queue ID passed to message handler.");
            return null;
        }

        private bool HasSpecifiedPosition(string[] callbackData)
        {
            return callbackData.Length == 3;
        }
    }
}

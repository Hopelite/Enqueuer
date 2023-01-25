using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Queue = Enqueuer.Persistence.Models.Queue;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    public class GetQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
    {
        private readonly IQueueService _queueService;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userService">User service to use.</param>
        public GetQueueCallbackHandler(IQueueService queueService, IUserService userService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            _queueService = queueService;
            _userService = userService;
        }

        public override string Command => CallbackConstants.GetQueueCommand;

        protected override async Task<Message> HandleCallbackAsyncImplementation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = _queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    var returnButton = GetReturnToChatButton(callbackData);
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "تم حذف هذه القائمة .",
                        replyMarkup: returnButton);
                }

                return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = _userService.GetUserByUserId(callbackQuery.From.Id);
            var replyMarkup = await BuildReplyMarkup(botClient, user, queue, callbackData);
            var responseMessage = BuildResponseMessage(queue);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                responseMessage,
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private async Task<InlineKeyboardMarkup> BuildReplyMarkup(ITelegramBotClient botClient, User user, Queue queue, CallbackData callbackData)
        {
            var replyMarkupButtons = new List<InlineKeyboardButton[]>()
            {
                user.IsParticipatingIn(queue)
                ? new InlineKeyboardButton[] { GetQueueRelatedButton("أخرجني من القائمة", CallbackConstants.DequeueMeCommand, callbackData, queue.Id) }
                : new InlineKeyboardButton[] { GetQueueRelatedButton("ضعني في القائمة", CallbackConstants.EnqueueCommand, callbackData, queue.Id) }
            };

            if (queue.IsQueueCreator(user) || await botClient.IsChatAdmin(user.UserId, queue.Chat.ChatId))
            {
                replyMarkupButtons.Add(new InlineKeyboardButton[] 
                {
                    GetRemoveQueueButton("إزالة القائمة", callbackData),
                    //GetDynamicQueueButton(CallbackConstants.SwitchQueueDynamicCommand, callbackData, queue)
                });
            }

            replyMarkupButtons.Add(new InlineKeyboardButton[] { GetRefreshButton(callbackData) });
            replyMarkupButtons.Add(new InlineKeyboardButton[] { GetReturnToChatButton(callbackData) });
            return new InlineKeyboardMarkup(replyMarkupButtons);
        }

        private InlineKeyboardButton GetQueueRelatedButton(string buttonText, string command, CallbackData callbackData, int queueId)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = command,
                ChatId = callbackData.ChatId,
                QueueData = new QueueData()
                { 
                    QueueId = queueId,
                }
            };

            var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        }

        private InlineKeyboardButton GetDynamicQueueButton(string command, CallbackData callbackData, Queue queue)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = command,
                ChatId = callbackData.ChatId,
                QueueData = new QueueData()
                {
                    QueueId = queue.Id,
                }
            };

            var buttonText = queue.IsDynamic ? "Make static" : "Make dynamic";
            var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        }

        private static string BuildResponseMessage(Queue queue)
        {
            if (!queue.Users.Any())
            {
                //if (queue.IsDynamic)
                //{
                //    return $"Queue <b>'{queue.Name}'</b> has no participants.\nQueue is <i>dynamic</i>";
                //}

                return $"لا تحتوي القائمة \"{queue.Name}\" على مشاركين.";
            }

            var responseMessage = new StringBuilder($"تضم القائمة \"{queue.Name}\" هؤلاء المشاركين:\n");
            foreach (var queueParticipant in queue.Users.OrderBy(userInQueue => userInQueue.Position))
            {
                responseMessage.AppendLine($"<b>{queueParticipant.User.FirstName} {queueParticipant.User.LastName}</b> ({queueParticipant.Position}");
            }

            //if (queue.IsDynamic)
            //{
            //    responseMessage.AppendLine($"Queue is <i>dynamic</i>");
            //}

            return responseMessage.ToString();
        }
    }
}

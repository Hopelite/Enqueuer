using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class GetChatCallbackHandler : CallbackHandlerBase
    {
        private const string UnableToCreateQueueMessage = "\nحاليًا ، يمكنك إنشاء قوائم انتظار فقط عن طريق كتابة الأمر \"/creerliste\" في هذه المحادثة ، لكنني سأتعلم كيفية إنشائها في الرسائل المباشرة قريبًا!";
        private readonly IChatService _chatService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChatCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public GetChatCallbackHandler(IChatService chatService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            _chatService = chatService;
        }

        public override string Command => CallbackConstants.GetChatCommand;

        protected override async Task<Message> HandleCallbackAsyncImplementation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            var chatQueues = _chatService.GetChatById(callbackData.ChatId)?.Queues.ToList();
            if (chatQueues is null)
            {
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "تم حذف هذه المحادثة.",
                        replyMarkup: GetReturnButton());
            }

            var responseMessage = (chatQueues.Count == 0
                ? "هذه المحادثة ليس لديها قوائم انتظار. هل تفكر في إنشاء واحدة ؟"
                : "هذه المحادثة لديها قوائم الانتظار هذه. يمكنك إدارة أي منهم عن طريق تحديدها.")
                + UnableToCreateQueueMessage;

            var replyMarkup = BuildReplyMarkup(chatQueues, callbackData, callbackData.ChatId);
            return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    responseMessage,
                    ParseMode.Html,
                    replyMarkup: replyMarkup);
        }

        private InlineKeyboardMarkup BuildReplyMarkup(List<Queue> chatQueues, CallbackData callbackData, int chatId)
        {
            var replyButtons = new InlineKeyboardButton[chatQueues.Count + 2][];
            for (int i = 0; i < chatQueues.Count; i++)
            {
                var newCallbackData = new CallbackData()
                {
                    Command = CallbackConstants.GetQueueCommand,
                    ChatId = chatId,
                    QueueData = new QueueData()
                    {
                        QueueId = chatQueues[i].Id,
                    },
                };

                var serializedCallbackData = DataSerializer.Serialize(newCallbackData);
                replyButtons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"'{chatQueues[i].Name}'", serializedCallbackData) };
            }

            replyButtons[^2] = new InlineKeyboardButton[] { GetRefreshButton(callbackData) };
            replyButtons[^1] = new InlineKeyboardButton[] { GetReturnButton() };
            return new InlineKeyboardMarkup(replyButtons);
        }

        private InlineKeyboardButton GetReturnButton()
        {
            var callbackData = new CallbackData()
            {
                Command = CallbackConstants.ListChatsCommand,
            };

            var serializedCallbackData = this.DataSerializer.Serialize(callbackData);
            return InlineKeyboardButton.WithCallbackData("Return", serializedCallbackData);
        }
    }
}

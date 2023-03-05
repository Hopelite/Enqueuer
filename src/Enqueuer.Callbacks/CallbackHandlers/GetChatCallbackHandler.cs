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
        private const string UnableToCreateQueueMessage = "\n<i>Currently, you can create queues only by writting the '<b>/createqueue</b>' command in this chat, but I'll learn how to create them in direct messages soon!</i>";
        private readonly IGroupService _chatService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChatCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public GetChatCallbackHandler(IGroupService chatService, IDataSerializer dataSerializer)
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
                        "This chat has been deleted.",
                        replyMarkup: GetReturnButton());
            }

            var responseMessage = (chatQueues.Count == 0
                ? "This chat has no queues. Are you thinking of creating one?"
                : "This chat has these queues. You can manage any one of them be selecting it.")
                + UnableToCreateQueueMessage;

            var replyMarkup = BuildReplyMarkup(chatQueues, callbackData, callbackData.ChatId);
            return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    responseMessage,
                    ParseMode.Html,
                    replyMarkup: replyMarkup);
        }

        private InlineKeyboardMarkup BuildReplyMarkup(List<Queue> chatQueues, CallbackData callbackData, long chatId)
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

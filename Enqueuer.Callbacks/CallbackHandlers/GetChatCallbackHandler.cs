using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class GetChatCallbackHandler : ICallbackHandler
    {
        private static readonly InlineKeyboardButton ReturnButton = InlineKeyboardButton.WithCallbackData("Return", "/viewchats");
        private readonly IChatService chatService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChatCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        public GetChatCallbackHandler(IChatService chatService)
        {
            this.chatService = chatService;
        }

        /// <inheritdoc/>
        public string Command => "/getchat";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/getchat' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data.SplitToWords();
            if (long.TryParse(callbackData[1], out var chatId))
            {
                var chatQueues = this.chatService.GetChatByChatId(chatId)?.Queues.ToList();
                if (chatQueues is null)
                {
                    return await botClient.EditMessageTextAsync(
                            callbackQuery.Message.Chat,
                            callbackQuery.Message.MessageId,
                            "This chat has been deleted.",
                            replyMarkup: ReturnButton);
                }

                var responceMessage = chatQueues.Count == 0
                    ? "This chat has no queues. Are you thinking of creating one?"
                    : "This chat has these queues. You can manage any one of them be selecting it.";

                var replyMarkup = BuildReplyMarkup(chatQueues, chatId);
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        responceMessage,
                        replyMarkup: replyMarkup);
            }

            throw new CallbackMessageHandlingException("Invalid chat ID passed to message handler.");
        }

        private static InlineKeyboardMarkup BuildReplyMarkup(List<Queue> chatQueues, long chatId)
        {
            var replyButtons = new InlineKeyboardButton[chatQueues.Count + 1][];
            for (int i = 0; i < chatQueues.Count; i++)
            {
                replyButtons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"'{chatQueues[i].Name}'", $"/getqueue {chatQueues[i].Id} {chatId}") };
            }

            replyButtons[^1] = new InlineKeyboardButton[] { ReturnButton };
            return new InlineKeyboardMarkup(replyButtons);
        }
    }
}

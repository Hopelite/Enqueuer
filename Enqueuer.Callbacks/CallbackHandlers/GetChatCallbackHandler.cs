using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class GetChatCallbackHandler : ICallbackHandler
    {
        private readonly IChatService chatService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChatCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="logger">Logger to log errors.</param>
        public GetChatCallbackHandler(IChatService chatService, ILogger logger)
        {
            this.chatService = chatService;
            this.logger = logger;
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
                var chatQueues = this.chatService.GetChatByChatId(chatId).Queues.ToList();
                var responceMessage = chatQueues.Count == 0
                    ? "This chat has no queues. Are you thinking of creating one?"
                    : "This chat has these queues. You can manage any one of them be selecting it.";

                var replyMarkup = new InlineKeyboardButton[chatQueues.Count + 1];
                for (int i = 0; i < chatQueues.Count; i++)
                {
                    replyMarkup[i] = InlineKeyboardButton.WithCallbackData(chatQueues[i].Name, $"/getqueue {chatQueues[i].Id} {chatId}");
                }
                
                replyMarkup[^1] = InlineKeyboardButton.WithCallbackData("Return", "/viewchats");
                return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        responceMessage,
                        replyMarkup: replyMarkup);
            }

            this.logger.LogError("Invalid chat ID passed to message handler.");
            return null;
        }
    }
}

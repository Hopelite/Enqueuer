using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.PrivateChatMessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/start' command.
    /// </summary>
    public class StartPrivateMessageHandler : IPrivateChatMessageHandler
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartPrivateMessageHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        public StartPrivateMessageHandler(IUserService userService)
        {
            this.userService = userService;
        }

        /// <inheritdoc/>
        public string Command => "/start";

        /// <summary>
        /// Handles incoming <see cref="Message"/> from private chat with '/start' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message">Incoming <see cref="Message"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            await this.userService.GetNewOrExistingUserAsync(message.From);

            var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("View chats", "/viewchats"),
            });

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "And your personal queue manager too!\n"
                + "Start by pressing the button below:",
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }
    }
}

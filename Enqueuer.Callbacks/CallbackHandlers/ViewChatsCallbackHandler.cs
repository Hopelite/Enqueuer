using Enqueuer.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class ViewChatsCallbackHandler : ICallbackHandler
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewChatsCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        public ViewChatsCallbackHandler(IUserService userService)
        {
            this.userService = userService;
        }

        /// <inheritdoc/>
        public string Command => "/viewchats";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/viewchats' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chats = this.userService.GetUserChats(callbackQuery.From.Id);
            var replyButtons = new List<InlineKeyboardButton>();
            foreach (var chat in chats)
            {
                replyButtons.Add(InlineKeyboardButton.WithCallbackData($"{chat.Name}", $"/getchat {chat.ChatId}"));
            }

            var replyMarkup = new InlineKeyboardMarkup(replyButtons);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                "Bot knows that you do participate in these chats. If one of the chats is not presented, please write any command in this chat, so bot will notice you.",
                replyMarkup: replyMarkup);
        }
    }
}

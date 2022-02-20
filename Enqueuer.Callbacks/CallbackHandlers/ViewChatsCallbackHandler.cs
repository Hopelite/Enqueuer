using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class ViewChatsCallbackHandler : ICallbackHandler
    {
        private const int MaxChatsPerRow = 2;
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

        /// <inheritdoc/>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chats = this.userService.GetUserChats(callbackQuery.From.Id).ToArray();
            var replyMarkup = BuildReplyMarkup(chats);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                "I know that you do participate in these chats. If one of the chats is not presented, please write any command in this chat, and I'll notice you there.",
                replyMarkup: replyMarkup);
        }

        private static InlineKeyboardMarkup BuildReplyMarkup(Chat[] chats)
        {
            var buttonsAtTheLastRow = chats.Length % MaxChatsPerRow;
            var rowsTotal = chats.Length / MaxChatsPerRow + buttonsAtTheLastRow;
            var chatsIndex = 0;

            var replyButtons = new InlineKeyboardButton[rowsTotal][];
            for (int i = 0; i < rowsTotal - 1; i++)
            {
                replyButtons[i] = new InlineKeyboardButton[MaxChatsPerRow];
                AddButtonsRow(replyButtons, i, MaxChatsPerRow, chats, ref chatsIndex);
            }

            AddLastButtonsRow(replyButtons, rowsTotal, buttonsAtTheLastRow, chats, chatsIndex);
            return new InlineKeyboardMarkup(replyButtons);
        }

        private static void AddLastButtonsRow(InlineKeyboardButton[][] replyButtons, int rowsTotal, int buttonsAtTheLastRow, Chat[] chats, int chatsIndex)
        {
            buttonsAtTheLastRow = buttonsAtTheLastRow == 0 ? MaxChatsPerRow : buttonsAtTheLastRow;
            replyButtons[^1] = new InlineKeyboardButton[buttonsAtTheLastRow];
            AddButtonsRow(replyButtons, rowsTotal - 1, buttonsAtTheLastRow, chats, ref chatsIndex);
        }

        private static void AddButtonsRow(InlineKeyboardButton[][] replyButtons, int row, int rowLength, Chat[] chats, ref int chatIndex)
        {
            for (int i = 0; i < rowLength; i++, chatIndex++)
            {
                replyButtons[row][i] = InlineKeyboardButton.WithCallbackData($"{chats[chatIndex].Name}", $"/getchat {chats[chatIndex].ChatId}");
            }
        }
    }
}

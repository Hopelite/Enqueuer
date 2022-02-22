using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class ListChatsCallbackHandler : CallbackHandlerBase
    {
        private const int MaxChatsPerRow = 2;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListChatsCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public ListChatsCallbackHandler(IUserService userService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.userService = userService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.ListChatsCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            var chats = this.userService.GetUserChats(callbackQuery.From.Id).ToList();
            if (chats.Count == 0)
            {
                return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    "I haven't seen you before. " +
                    "Please, write any command in any chat with me, and I'll notice you there. " +
                    "Then come here and write <b>'/start'</b> again.",
                    ParseMode.Html);
            }

            var replyMarkup = this.BuildReplyMarkup(chats);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                "I know that you do participate in these chats. " +
                "If one of the chats is not presented, please write any command in this chat, and I'll notice you there.",
                replyMarkup: replyMarkup);
        }

        private InlineKeyboardMarkup BuildReplyMarkup(List<Chat> chats)
        {
            var buttonsAtTheLastRow = chats.Count % MaxChatsPerRow;
            var rowsTotal = chats.Count / MaxChatsPerRow + buttonsAtTheLastRow;
            var chatsIndex = 0;

            var replyButtons = new InlineKeyboardButton[rowsTotal][];
            for (int i = 0; i < rowsTotal - 1; i++)
            {
                replyButtons[i] = new InlineKeyboardButton[MaxChatsPerRow];
                this.AddButtonsRow(replyButtons, i, MaxChatsPerRow, chats, ref chatsIndex);
            }

            this.AddLastButtonsRow(replyButtons, rowsTotal, buttonsAtTheLastRow, chats, chatsIndex);
            return new InlineKeyboardMarkup(replyButtons);
        }

        private void AddLastButtonsRow(InlineKeyboardButton[][] replyButtons, int rowsTotal, int buttonsAtTheLastRow, List<Chat> chats, int chatsIndex)
        {
            buttonsAtTheLastRow = buttonsAtTheLastRow == 0 ? MaxChatsPerRow : buttonsAtTheLastRow;
            replyButtons[^1] = new InlineKeyboardButton[buttonsAtTheLastRow];
            AddButtonsRow(replyButtons, rowsTotal - 1, buttonsAtTheLastRow, chats, ref chatsIndex);
        }

        private void AddButtonsRow(InlineKeyboardButton[][] replyButtons, int row, int rowLength, List<Chat> chats, ref int chatIndex)
        {
            for (int i = 0; i < rowLength; i++, chatIndex++)
            {
                var callbackData = new CallbackData()
                {
                    Command = CallbackConstants.GetChatCommand,
                    ChatId = chats[chatIndex].ChatId,
                };

                var serializedCallbackData = this.dataSerializer.Serialize(callbackData);
                replyButtons[row][i] = InlineKeyboardButton.WithCallbackData($"{chats[chatIndex].Name}", serializedCallbackData);
            }
        }
    }
}

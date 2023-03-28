using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class ListChatsCallbackHandler : CallbackHandlerBase
{
    private const int MaxChatsPerRow = 2;
    private readonly IGroupService _groupService;

    public ListChatsCallbackHandler(ITelegramBotClient telegramBotClient, IGroupService groupService, IDataSerializer dataSerializer, IMessageProvider messageProvider)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _groupService = groupService;
    }

    protected override async Task HandleAsyncImplementation(Callback callback)
    {
        var groups = (await _groupService.GetUserGroups(callback.From.Id, CancellationToken.None)).ToList();
        if (groups.Count == 0)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_UserDoesNotParticipateInAnyGroup_Message),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[] { GetRefreshButton(callback.CallbackData) }));

            return;
        }

        var replyMarkup = BuildReplyMarkup(groups);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_ListChats_Message),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private InlineKeyboardMarkup BuildReplyMarkup(List<Group> chats)
    {
        var buttonsAtTheLastRow = chats.Count % MaxChatsPerRow;
        var rowsTotal = chats.Count / MaxChatsPerRow + buttonsAtTheLastRow;
        var chatsIndex = 0;

        var replyButtons = new InlineKeyboardButton[rowsTotal][];
        for (int i = 0; i < rowsTotal - 1; i++)
        {
            replyButtons[i] = new InlineKeyboardButton[MaxChatsPerRow];
            AddButtonsRow(replyButtons, i, MaxChatsPerRow, chats, ref chatsIndex);
        }

        // TODO: add GetRefreshButton(callback.CallbackData) to the last row

        AddLastButtonsRow(replyButtons, rowsTotal, buttonsAtTheLastRow, chats, chatsIndex);
        return new InlineKeyboardMarkup(replyButtons);
    }

    private void AddLastButtonsRow(InlineKeyboardButton[][] replyButtons, int rowsTotal, int buttonsAtTheLastRow, List<Group> chats, int chatsIndex)
    {
        buttonsAtTheLastRow = buttonsAtTheLastRow == 0 ? MaxChatsPerRow : buttonsAtTheLastRow;
        replyButtons[^1] = new InlineKeyboardButton[buttonsAtTheLastRow];
        AddButtonsRow(replyButtons, rowsTotal - 1, buttonsAtTheLastRow, chats, ref chatsIndex);
    }

    private void AddButtonsRow(InlineKeyboardButton[][] replyButtons, int row, int rowLength, List<Group> chats, ref int chatIndex)
    {
        for (int i = 0; i < rowLength; i++, chatIndex++)
        {
            var callbackData = new CallbackData()
            {
                Command = CallbackConstants.GetChatCommand,
                ChatId = chats[chatIndex].Id,
            };

            var serializedCallbackData = DataSerializer.Serialize(callbackData);
            replyButtons[row][i] = InlineKeyboardButton.WithCallbackData(chats[chatIndex].Title, serializedCallbackData);
        }
    }
}

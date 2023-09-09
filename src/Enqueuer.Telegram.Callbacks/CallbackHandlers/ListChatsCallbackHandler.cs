using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class ListChatsCallbackHandler : CallbackHandlerBase
{
    private const int MaxChatsPerRow = 2;
    private readonly IGroupService _groupService;

    public ListChatsCallbackHandler(ITelegramBotClient telegramBotClient, IGroupService groupService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _groupService = groupService;
    }

    protected override async Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetUserGroups(callback.From.Id, CancellationToken.None)).ToList();
        if (groups.Count == 0)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.Callback_ListChats_UserDoesNotParticipateInAnyGroup_Message, MessageParameters.None),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[] { GetRefreshButton(callback.CallbackData) }),
                cancellationToken: cancellationToken);

            return;
        }

        var replyMarkup = BuildReplyMarkup(groups);
        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.Callback_ListChats_DisplayChatsList_Message, MessageParameters.None),
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private InlineKeyboardMarkup BuildReplyMarkup(List<Group> chats)
    {
        var buttonsAtTheLastRow = chats.Count % MaxChatsPerRow;
        var rowsTotal = chats.Count / MaxChatsPerRow + buttonsAtTheLastRow;
        var chatsIndex = 0;

        var replyButtons = new InlineKeyboardButton[rowsTotal][];
        for (var i = 0; i < rowsTotal - 1; i++)
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
        for (var i = 0; i < rowLength; i++, chatIndex++)
        {
            var callbackData = new CallbackData()
            {
                Command = CallbackCommands.GetChatCommand,
                TargetChatId = chats[chatIndex].Id,
            };

            var serializedCallbackData = DataSerializer.Serialize(callbackData);
            replyButtons[row][i] = InlineKeyboardButton.WithCallbackData(chats[chatIndex].Title, serializedCallbackData);
        }
    }
}

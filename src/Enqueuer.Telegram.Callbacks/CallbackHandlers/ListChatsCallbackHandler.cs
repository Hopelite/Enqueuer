using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class ListChatsCallbackHandler : CallbackHandlerBase
{
    private const int MaxChatsPerRow = 2;
    private readonly IGroupService _groupService;
    private readonly ICallbackDataSerializer _dataSerializer;

    public ListChatsCallbackHandler(ITelegramBotClient telegramBotClient, IGroupService groupService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, localizationProvider)
    {
        _groupService = groupService;
        _dataSerializer = dataSerializer;
    }

    protected override async Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetUserGroups(callbackContext.Sender.Id, CancellationToken.None)).ToList();
        if (groups.Count == 0)
        {
            var returnButton = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithRefreshButton(callbackContext.CallbackData)
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.Callback_ListChats_UserDoesNotParticipateInAnyGroup_Message, MessageParameters.None),
                ParseMode.Html,
                replyMarkup: returnButton,
                cancellationToken: cancellationToken);

            return;
        }

        var replyMarkup = BuildReplyMarkup(groups, callbackContext.CallbackData);
        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.ListChatsCallbackHandler.Callback_ListChats_DisplayChatsList_Message, MessageParameters.None),
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private InlineKeyboardMarkup BuildReplyMarkup(List<Group> chats, CallbackData callbackData)
    {
        var buttonsAtTheLastRow = chats.Count % MaxChatsPerRow;
        var rowsTotal = chats.Count / MaxChatsPerRow + buttonsAtTheLastRow;
        var chatsIndex = 0;

        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider);
        for (var i = 0; i < rowsTotal - 1; i++)
        {
            AddButtonsRow(replyMarkup, MaxChatsPerRow, chats, ref chatsIndex);
        }

        AddLastButtonsRow(replyMarkup, buttonsAtTheLastRow, chats, chatsIndex);
        return replyMarkup
            .FromNewRow()
            .WithRefreshButton(callbackData)
            .Build();
    }

    private void AddLastButtonsRow(ReplyMarkupBuilder markupBuilder, int buttonsAtTheLastRow, List<Group> chats, int chatsIndex)
    {
        buttonsAtTheLastRow = buttonsAtTheLastRow == 0 ? MaxChatsPerRow : buttonsAtTheLastRow;
        AddButtonsRow(markupBuilder, buttonsAtTheLastRow, chats, ref chatsIndex);
    }

    private static void AddButtonsRow(ReplyMarkupBuilder markupBuilder, int rowLength, List<Group> chats, ref int chatIndex)
    {
        markupBuilder.FromNewRow();
        for (var i = 0; i < rowLength; i++, chatIndex++)
        {
            markupBuilder.WithOpenChatButton(chats[chatIndex]);
        }
    }
}

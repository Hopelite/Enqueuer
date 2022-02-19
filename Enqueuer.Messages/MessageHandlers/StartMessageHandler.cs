using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Configuration;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class StartMessageHandler : IMessageHandler
    {
        private readonly IBotConfiguration botConfiguration;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartMessageHandler"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IBotConfiguration"/> with bot configuration.</param>
        public StartMessageHandler(IBotConfiguration botConfiguration, IUserService userService)
        {
            this.botConfiguration = botConfiguration;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public string Command => "/start";

        /// <inheritdoc/>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            await this.userService.GetNewOrExistingUserAsync(message.From);
            if (message.IsPrivateChat())
            {
                var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("View chats", "/viewchats"),
                });

                return await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                    + "And your personal queue manager too!\n"
                    + "Start by pressing the button below:",
                    ParseMode.Html,
                    replyMarkup: viewChatsButton);
            }

            return await botClient.SendTextMessageAsync(
                message.Chat,
                "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "To get the list of commands, write '<b>/help</b>'.\n"
                + "<i>Please, message this guy (@hopelite) to get help, give feedback or report a bug.</i>\n"
                + $"\n<i>Bot version: {this.botConfiguration.BotVersion}</i>",
                ParseMode.Html);
        }
    }
}

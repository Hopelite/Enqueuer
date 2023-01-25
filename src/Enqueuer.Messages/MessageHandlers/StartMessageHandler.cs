using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Services.Interfaces;
using Enqueuer.Data.Configuration;
using Enqueuer.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class StartMessageHandler : MessageHandlerBase
    {
        private readonly IBotConfiguration botConfiguration;
        private readonly IDataSerializer dataSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartMessageHandler"/> class.
        /// </summary>
        /// <param name="botConfiguration"><see cref="IBotConfiguration"/> with bot configuration.</param>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataDeserializer"/> to serialize data for callback handlers.</param>
        public StartMessageHandler(
            IBotConfiguration botConfiguration,
            IChatService chatService,
            IUserService userService,
            IDataSerializer dataSerializer)
            : base(chatService, userService)
        {
            this.botConfiguration = botConfiguration;
            this.dataSerializer = dataSerializer;
        }

        /// <inheritdoc/>
        public override string Command => MessageConstants.StartCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.IsPrivateChat())
            {
                await this.userService.GetNewOrExistingUserAsync(message.From);
                var callbackButtonData = new CallbackData()
                {
                    Command = CallbackConstants.ListChatsCommand,
                };

                var serializedCallbackData = this.dataSerializer.Serialize(callbackButtonData);
                var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("عرض المحادثات", serializedCallbackData),
                });

                return await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "أهلاً بك! أنا Enqueuer Bot ، روبوت لإنشاء وإدارة قوائم الانتظار. ومدير قائمة الانتظار الشخصي أيضًا! ابدأ بالضغط على الزر أدناه:",
                    ParseMode.Html,
                    replyMarkup: viewChatsButton);
            }

            await this.GetNewOrExistingUserAndChat(message);
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

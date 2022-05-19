using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <summary>
    /// Contains basic implementation for message handlers.
    /// </summary>
    public abstract class MessageHandlerBase : IMessageHandler
    {
        protected readonly IChatService chatService;
        protected readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueMessageHandler"/> class.
        /// </summary>
        public MessageHandlerBase(IChatService chatService, IUserService userService)
        {
            this.chatService = chatService;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public abstract string Command { get; }

        /// <inheritdoc/>
        public abstract Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message);

        /// <summary>
        /// Gets new or existing <see cref="User"/> and <see cref="Chat"/>.
        /// </summary>
        /// <param name="message"><see cref="Message"/> to get user and chat by.</param>
        /// <returns>New or existing user and chat.</returns>
        protected async Task<(User user, Chat chat)> GetNewOrExistingUserAndChat(Message message)
        {
            var chat = await this.chatService.GetNewOrExistingChatAsync(message.Chat);
            var user = await this.userService.GetNewOrExistingUserAsync(message.From);
            await this.chatService.AddUserToChatIfNotAlready(user, chat);

            return (user, chat);
        }
    }
}

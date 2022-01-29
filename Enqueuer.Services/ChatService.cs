using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Services
{
    /// <inheritdoc/>
    public class ChatService : IChatService
    {
        private readonly IRepository<Chat> chatRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatService"/> class.
        /// </summary>
        /// <param name="chatRepository"><see cref="IRepository{T}"/> with <see cref="Chat"/> entities.</param>
        public ChatService(IRepository<Chat> chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        /// <inheritdoc/>
        public async Task<Chat> GetNewOrExistingChatAsync(Telegram.Bot.Types.Chat telegramChat)
        {
            var chat = this.chatRepository.GetAll()
                .FirstOrDefault(chat => chat.ChatId == telegramChat.Id);

            if (chat is null)
            {
                await this.chatRepository.AddAsync(telegramChat);
                return telegramChat;
            }

            return chat;
        }
    }
}

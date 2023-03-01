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
        public async Task<Chat> GetOrCreateChatAsync(Telegram.Bot.Types.Chat telegramChat)
        {
            var chat = this.GetChatByTelegramChatId(telegramChat.Id);

            if (chat is null)
            {
                await this.chatRepository.AddAsync(telegramChat);
                return this.GetChatByTelegramChatId(telegramChat.Id);
            }

            return chat;
        }

        /// <inheritdoc/>
        public async Task AddUserToChatIfNotAlready(User user, Chat chat)
        {
            if (chat.Users.FirstOrDefault(chatUser => chatUser.UserId == user.UserId) is null)
            {
                chat.Users.Add(user);
                await this.chatRepository.UpdateAsync(chat);
            }
        }

        /// <inheritdoc/>
        public int GetNumberOfQueues(long chatId)
        {
            return this.chatRepository.GetAll()
                .First(chat => chat.ChatId == chatId)
                .Queues.Count;
        }

        /// <inheritdoc/>
        public Chat GetChatByTelegramChatId(long chatId)
        {
            return this.chatRepository.GetAll()
                    .FirstOrDefault(chat => chat.ChatId == chatId);
        }

        public Chat GetChatById(int id)
        {
            return this.chatRepository.Get(id);
        }
    }
}

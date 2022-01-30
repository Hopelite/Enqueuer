using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var chat = this.GetChatByChatId(telegramChat.Id);

            if (chat is null)
            {
                await this.chatRepository.AddAsync(telegramChat);
                return this.GetChatByChatId(telegramChat.Id);
            }

            return chat;
        }

        /// <inheritdoc/>
        public async Task AddUserToChat(User user, Chat chat)
        {
            if (chat.Users.FirstOrDefault(chatUser => chatUser.UserId == user.UserId) is null)
            {
                chat.Users.Add(user);
                await this.chatRepository.UpdateAsync(chat);
            }
        }

        private Chat GetChatByChatId(long chatId)
        {
            return this.chatRepository.GetAll()
                    .Include(chat => chat.Users)
                    .Include(chat => chat.Queues)
                    .AsNoTracking()
                    .FirstOrDefault(chat => chat.ChatId == chatId);
        }
    }
}

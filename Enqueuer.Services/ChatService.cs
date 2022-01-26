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
        public async Task<Chat> GetNewOrExistingChat(long chatId)
        {
            var chat = this.chatRepository.GetAll()
                .FirstOrDefault(chat => chat.ChatId == chatId);

            if (chat is null)
            {
                chat = new Chat()
                {
                    ChatId = chatId,
                };

                await this.chatRepository.AddAsync(chat);
            }

            return chat;
        }
    }
}

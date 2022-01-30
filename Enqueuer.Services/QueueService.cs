using System.Collections.Generic;
using System.Linq;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enqueuer.Services
{
    /// <inheritdoc/>
    public class QueueService : IQueueService
    {
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueService"/> class.
        /// </summary>
        /// <param name="chatRepository"><see cref="IRepository{T}"/> with <see cref="Queue"/> entities.</param>
        public QueueService(IRepository<Queue> queueRepository)
        {
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public Queue GetChatQueueByName(string name, long chatId)
        {
            return this.queueRepository.GetAll()
                .Include(queue => queue.Creator)
                .Include(queue => queue.Users)
                .AsNoTracking()
                .FirstOrDefault(queue => queue.Chat.ChatId == chatId && queue.Name.Equals(name));
        }

        /// <inheritdoc/>
        public IEnumerable<Queue> GetChatQueues(int chatId)
        {
            return this.queueRepository.GetAll()
                .AsNoTracking()
                .Where(queue => queue.ChatId == chatId);
        }

        /// <inheritdoc/>
        public IEnumerable<Queue> GetTelegramChatQueues(long chatId)
        {
            return this.queueRepository.GetAll()
                .Include(queue => queue.Chat)
                .AsNoTracking()
                .Where(queue => queue.Chat.ChatId == chatId);
        }
    }
}

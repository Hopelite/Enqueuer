using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Services
{
    public class QueueService : IQueueService
    {
        private readonly IRepository<Queue> _queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueService"/> class.
        /// </summary>
        /// <param name="queueRepository"><see cref="IRepository{T}"/> with <see cref="Queue"/> entities.</param>
        public QueueService(IRepository<Queue> queueRepository)
        {
            _queueRepository = queueRepository;
        }

        public Queue GetChatQueueByName(string name, long chatId)
        {
            return _queueRepository.GetAll()
                .FirstOrDefault(queue => queue.Chat.ChatId == chatId && queue.Name.Equals(name));
        }

        public IEnumerable<Queue> GetChatQueues(int chatId)
        {
            return _queueRepository.GetAll()
                .Where(queue => queue.ChatId == chatId);
        }

        public IEnumerable<Queue> GetTelegramChatQueues(long chatId)
        {
            return _queueRepository.GetAll()
                .Where(queue => queue.Chat.ChatId == chatId);
        }

        public Queue GetQueueById(int id)
        {
            return _queueRepository.Get(id);
        }

        public async Task RemoveUserAsync(Queue queue, User user)
        {
            var userToRemove = queue.Users.FirstOrDefault(queueUser => queueUser.UserId == user.Id);
            if (userToRemove is not null)
            {
                queue.Users.Remove(userToRemove);
                await _queueRepository.UpdateAsync(queue);
            }
        }

        public async Task DeleteQueueAsync(Queue queue)
        {
            await _queueRepository.DeleteAsync(queue);
        }

        public async Task UpdateQueueAsync(Queue queue)
        {
            await _queueRepository.UpdateAsync(queue);
        }

        public Task AddAsync(Queue queue)
        {
            return _queueRepository.AddAsync(queue);
        }
    }
}

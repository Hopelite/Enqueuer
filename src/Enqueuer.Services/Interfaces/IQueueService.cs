using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="Queue"/>.
    /// </summary>
    public interface IQueueService
    {
        /// <summary>
        /// Gets queues belonging to <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
        public IEnumerable<Queue> GetChatQueues(int chatId);

        /// <summary>
        /// Gets queues belonging to <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
        public IEnumerable<Queue> GetTelegramChatQueues(long chatId);

        /// <summary>
        /// Gets <see cref="Queue"/> by <paramref name="name"/> which belongs to <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <returns><see cref="Queue"/> with specified <paramref name="name"/> and <paramref name="chatId"/>; null if doesn't exist.</returns>
        public Queue GetChatQueueByName(string name, long chatId);

        /// <summary>
        /// Gets <see cref="Queue"/> by <paramref name="id"/>.
        /// </summary>
        /// <returns><see cref="Queue"/> with specified <paramref name="id"/>; null if doesn't exist.</returns>
        public Queue GetQueueById(int id);

        /// <summary>
        /// Removes <paramref name="user"/> from <paramref name="queue"/>.
        /// </summary>
        public Task RemoveUserAsync(Queue queue, User user);

        /// <summary>
        /// Deletes <paramref name="queue"/>.
        /// </summary>
        public Task DeleteQueueAsync(Queue queue);

        /// <summary>
        /// Updates <paramref name="queue"/>.
        /// </summary>
        public Task UpdateQueueAsync(Queue queue);

        Task AddAsync(Queue queue);
    }
}

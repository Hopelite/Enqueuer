using System.Collections.Generic;
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
        /// <param name="chatId">The ID of the <see cref="Chat"/> whose queues to get.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
        public IEnumerable<Queue> GetChatQueues(int chatId);

        /// <summary>
        /// Gets queues belonging to <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <param name="chatId">The ID of Telegram chat whose queues to get.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
        public IEnumerable<Queue> GetTelegramChatQueues(long chatId);

        /// <summary>
        /// Gets <see cref="Queue"/> by <paramref name="name"/> which belongs to <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <param name="name"><see cref="Chat"/> name to search for.</param>
        /// <param name="chatId">The ID of the Telegram chat whose queues to search.</param>
        /// <returns><see cref="Queue"/> with specified <paramref name="name"/> and <paramref name="chatId"/>; null if doesn't exist.</returns>
        public Queue GetChatQueueByName(string name, long chatId);
    }
}

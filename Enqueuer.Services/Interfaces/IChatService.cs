using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="Chat"/>.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Gets <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <param name="chatId">Telegram chat ID to get <see cref="Chat"/> by.</param>
        /// <returns></returns>
        public Chat GetChatByChatId(long chatId);

        /// <summary>
        /// Gets already existing <see cref="Chat"/> or creates new one.
        /// </summary>
        /// <param name="telegramChat">Telegram chat using which ID to get or create a new one.</param>
        /// <returns>Task with existing or newly created <see cref="Chat"/> in return.</returns>
        public Task<Chat> GetNewOrExistingChatAsync(Telegram.Bot.Types.Chat telegramChat);

        /// <summary>
        /// Adds <paramref name="user"/> to <paramref name="chat"/> if is not already added.
        /// </summary>
        /// <param name="user"><see cref="User"/> to add into <paramref name="chat"/>.</param>
        /// <param name="chat"><see cref="Chat"/> where <paramref name="user"/> should be added.</param>
        /// <returns>Task in return.</returns>
        public Task AddUserToChatIfNotAlready(User user, Chat chat);

        /// <summary>
        /// Gets number of <see cref="Queue"/> is <see cref="Chat"/> with specified <paramref name="chatId"/>.
        /// </summary>
        /// <param name="chatId">ID of the Telegram chat whose queues to count.</param>
        /// <returns>Number of queues in chat.</returns>
        public int GetNumberOfQueues(long chatId);
    }
}

using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Enqueuer.Callbacks.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="ITelegramBotClient"/>.
    /// </summary>
    public static class TelegramBotClientExtensions
    {
        /// <summary>
        /// Checks whether user with specified <paramref name="userId"/> is the chat admin.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to send request with.</param>
        /// <param name="userId">Telegram user ID to check.</param>
        /// <param name="chatId">Telegram chat ID to check.</param>
        /// <returns>True, if user with specified <paramref name="userId"/> is the chat admin; false otherwise.</returns>
        public static async Task<bool> IsChatAdmin(this ITelegramBotClient telegramBotClient, long userId, long chatId)
        {
            var chatAdmins = await telegramBotClient.GetChatAdministratorsAsync(chatId);
            return chatAdmins.Any(chatAdmin => chatAdmin.User.Id == userId);
        }
    }
}

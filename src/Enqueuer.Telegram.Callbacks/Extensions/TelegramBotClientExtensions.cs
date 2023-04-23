using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Enqueuer.Telegram.Callbacks.Extensions;

public static class TelegramBotClientExtensions
{
    /// <summary>
    /// Checks whether user with specified <paramref name="userId"/> is the chat admin.
    /// </summary>
    public static async Task<bool> IsChatAdmin(this ITelegramBotClient telegramBotClient, long userId, long chatId)
    {
        var chatAdmins = await telegramBotClient.GetChatAdministratorsAsync(chatId);
        return chatAdmins.Any(chatAdmin => chatAdmin.User.Id == userId);
    }
}

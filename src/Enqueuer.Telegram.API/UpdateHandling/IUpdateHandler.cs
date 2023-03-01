using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.UpdateHandling;

/// <summary>
/// Handles the incoming <see cref="Update"/> from Telegram.
/// </summary>
public interface IUpdateHandler
{
    /// <summary>
    /// Handles the incoming <paramref name="update"/>.
    /// </summary>
    Task HandleUpdateAsync(Update update);
}

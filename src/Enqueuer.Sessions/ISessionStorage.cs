using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Sessions.Types;

namespace Enqueuer.Sessions;

/// <summary>
/// Stores Telegram chat sessions.
/// </summary>
public interface ISessionStorage
{
    /// <summary>
    /// Gets an existing or creates a new <see cref="Session"/> with the specified <paramref name="chatId"/>.
    /// </summary>
    Task<Session> GetOrCreateSessionAsync(long chatId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the existing <paramref name="session"/> in storage.
    /// </summary>
    Task UpdateSessionsAsync(Session session, CancellationToken cancellationToken);
}

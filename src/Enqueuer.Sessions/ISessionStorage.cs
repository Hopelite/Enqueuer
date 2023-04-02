using System.Diagnostics.CodeAnalysis;
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
    /// Tries to get a <paramref name="session"/> with the specified <paramref name="chatId"/> if exists.
    /// </summary>
    Task<bool> TryGetSessionAsync(long chatId, [NotNullWhen(true)] out Session? session, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the existing <paramref name="session"/> in storage.
    /// </summary>
    Task UpdateSessionAsync(Session session, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a session with the specified <paramref name="chatId"/> from storage.
    /// </summary>
    Task StopSessionAsync(long chatId, CancellationToken cancellationToken);
}

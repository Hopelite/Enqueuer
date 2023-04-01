using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Sessions.Types;

namespace Enqueuer.Sessions;

/// <summary>
/// Stores Telegram sessions in memory.
/// </summary>
public class InMemorySessionStorage : ISessionStorage
{
    private readonly ConcurrentDictionary<long, Session> _sessions = new();

    public Task<Session> GetOrCreateSessionAsync(long chatId, CancellationToken cancellationToken)
    {
        if (!_sessions.TryGetValue(chatId, out var session))
        {
            session = new Session
            {
                ChatId = chatId,
            };
        }

        return Task.FromResult(session);
    }

    public Task UpdateSessionAsync(Session session, CancellationToken cancellationToken)
    {
        _sessions.AddOrUpdate(session.ChatId, session, (key, existingSession) => session);
        return Task.CompletedTask;
    }
}

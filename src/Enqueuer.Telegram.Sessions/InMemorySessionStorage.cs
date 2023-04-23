using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Sessions.Types;

namespace Enqueuer.Telegram.Sessions;

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

    public Task<bool> TryGetSessionAsync(long chatId, [NotNullWhen(true)] out Session? session, CancellationToken cancellationToken)
    {
        return Task.FromResult(_sessions.TryGetValue(chatId, out session));
    }

    public Task UpdateSessionAsync(Session session, CancellationToken cancellationToken)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        _sessions.AddOrUpdate(session.ChatId, session, (key, existingSession) => session);
        return Task.CompletedTask;
    }

    public Task StopSessionAsync(long chatId, CancellationToken cancellationToken)
    {
        _sessions.TryRemove(chatId, out var _);
        return Task.CompletedTask;
    }
}

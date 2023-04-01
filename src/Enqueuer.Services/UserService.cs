using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Services.Extensions;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services;

public class UserService : IUserService
{
    private readonly EnqueuerContext _enqueuerContext;

    public UserService(EnqueuerContext enqueuerContext)
    {
        _enqueuerContext = enqueuerContext;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<User> GetOrStoreUserAsync(Telegram.Bot.Types.User telegramUser, CancellationToken cancellationToken)
    {
        if (telegramUser == null)
        {
            throw new ArgumentNullException(nameof(telegramUser));
        }

        return GetOrStoreUserAsyncInternal(telegramUser, cancellationToken);
    }

    private async Task<User> GetOrStoreUserAsyncInternal(Telegram.Bot.Types.User telegramUser, CancellationToken cancellationToken)
    {
        var user = await _enqueuerContext.Users.FindAsync(new object[] { telegramUser.Id }, cancellationToken);
        if (user == null)
        {
            user = telegramUser.ConvertToEntity();
            _enqueuerContext.Add(user);
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        if (user.FirstName != telegramUser.FirstName || !string.Equals(user.LastName, telegramUser.LastName))
        {
            user.FirstName = telegramUser.FirstName;
            user.LastName = telegramUser.LastName;
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }

        return user;
    }
}

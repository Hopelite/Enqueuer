using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;
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
        }

        return user;
    }

    public User GetUserByUserId(long userId)
    {
        throw new NotImplementedException();
        //return this.userRepository.GetAll()
        //        .FirstOrDefault(user => user.UserId == userId);
    }

    /// <inheritdoc/>
    public IEnumerable<Group> GetUserChats(long userId)
    {
        throw new NotImplementedException();
        //return this.userRepository.GetAll()
        //    .FirstOrDefault(user => user.UserId == userId)?.Chats;
    }
}

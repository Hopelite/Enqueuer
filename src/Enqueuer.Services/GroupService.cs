using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Services;

public class GroupService : IGroupService
{
    private readonly EnqueuerContext _enqueuerContext;

    public GroupService(EnqueuerContext enqueuerContext)
    {
        _enqueuerContext = enqueuerContext;
    }

    public Task<Group?> GetAsync(long id, CancellationToken cancellationToken)
    {
        return _enqueuerContext.Groups
            .SingleOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException">Thrown if <paramref name="telegramGroup"/> is not a Group or Supergroup.</exception>
    public Task<Group> GetOrStoreGroupAsync(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken)
    {
        if (telegramGroup == null)
        {
            throw new ArgumentNullException(nameof(telegramGroup));
        }

        if (telegramGroup.Type != ChatType.Group || telegramGroup.Type != ChatType.Supergroup)
        {
            throw new ArgumentException($"{nameof(GroupService)} supports only {ChatType.Group} and {ChatType.Supergroup} chat types.", nameof(telegramGroup));
        }

        return GetOrStoreGroupAsyncInternal(telegramGroup, cancellationToken);
    }

    private async Task<Group> GetOrStoreGroupAsyncInternal(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken)
    {
        var group = await _enqueuerContext.FindAsync<Group>(new object[] { telegramGroup.Id }, cancellationToken);
        if (group == null)
        {
            group = telegramGroup.ConvertToGroup();
            _enqueuerContext.Add(group);
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }

        return group;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<(Group group, User user)> AddOrUpdateUserAndGroupAsync(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User user, bool includeQueues, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return AddOrUpdateUserToGroupAsyncInternal(telegramGroup, user, includeQueues, cancellationToken);
    }

    private async Task<(Group group, User user)> AddOrUpdateUserToGroupAsyncInternal(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User telegramUser, bool includeQueues, CancellationToken cancellationToken)
    {
        var group = await IncludeProperties().SingleOrDefaultAsync(g => g.Id == telegramGroup.Id, cancellationToken);
        if (group == null)
        {
            group = telegramGroup.ConvertToGroup();
            _enqueuerContext.Add(group);
        }

        var user = await _enqueuerContext.FindAsync<User>(new object[] { telegramUser.Id }, cancellationToken) ?? telegramUser.ConvertToEntity();
        if (!group.Members.Any(m => m.Id == user.Id))
        {
            group.Members.Add(user);
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }

        return (group, user);

        IQueryable<Group> IncludeProperties()
        {
            if (includeQueues)
            {
                return _enqueuerContext.Groups.Include(g => g.Members)
                        .Include(g => g.Queues);
            }

            return _enqueuerContext.Groups.Include(g => g.Members);
        }
    }


    public async Task AddUserToChatIfNotAlready(User user, Group chat)
    {
        if (chat.Members.FirstOrDefault(chatUser => chatUser.Id == user.Id) is null)
        {
            chat.Members.Add(user);
            //await this.chatRepository.UpdateAsync(chat);
        }
    }

    /// <inheritdoc/>
    public int GetNumberOfQueues(long chatId)
    {
        return 1;
        //return this.chatRepository.GetAll()
        //    .First(chat => chat.ChatId == chatId)
        //    .Queues.Count;
    }

    /// <inheritdoc/>
    //public Chat GetChatByTelegramChatId(long chatId)
    //{
    //    return this.chatRepository.GetAll()
    //            .FirstOrDefault(chat => chat.ChatId == chatId);
    //}
}

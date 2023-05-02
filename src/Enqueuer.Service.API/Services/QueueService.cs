using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Persistence;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.Messages;
using Enqueuer.Service.Messages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Service.API.Services;

public class QueueService : IQueueService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;

    public QueueService(IServiceScopeFactory scopeFactory, IMapper mapper)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
    }

    public async Task<QueueInfo?> GetQueueAsync(long groupId, int queueId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queue = await enqueuerContext.Queues
            .Include(q => q.Creator)
            .Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.GroupId == groupId && q.Id == queueId, cancellationToken);

        return queue == null
            ? null
            : _mapper.Map<QueueInfo>(queue);
    }

    public async Task<QueueInfo> CreateQueueAsync(long groupId, CreateQueueRequest request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var creator = await enqueuerContext.Users.FindAsync(new object[] { request.CreatorId }, cancellationToken);
        if (creator == null)
        {
            throw new UserDoesNotExistException($"User with the \"{request.CreatorId}\" ID does not exist.");
        }

        var group = await enqueuerContext.Groups.FindAsync(new object[] { groupId }, cancellationToken);
        if (group == null)
        {
            throw new GroupDoesNotExistException($"Group with the \"{groupId}\" ID does not exist.");
        }

        if (enqueuerContext.Queues.Any(q => q.GroupId == groupId && q.Name.Equals(request.QueueName)))
        {
            throw new QueueAlreadyExistsException($"Queue \"{request.QueueName}\" already exists in the group with the \"{groupId}\" ID.");
        }

        var queue = new Persistence.Models.Queue
        {
            Name = request.QueueName,
            Group = group,
            Creator = creator,
            Members = new List<Persistence.Models.QueueMember>(),
        };

        enqueuerContext.Queues.Add(queue);
        await enqueuerContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QueueInfo>(queue);
    }

    public async Task DeleteQueueAsync(long groupId, int queueId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queue = await enqueuerContext.Queues.FindAsync(new object[] { groupId, queueId }, cancellationToken);
        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        enqueuerContext.Queues.Remove(queue);
        await enqueuerContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<QueueMember?> GetQueueMemberAsync(long groupId, int queueId, long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queueMember = await enqueuerContext.QueueMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId && m.QueueId == queueId, cancellationToken);

        return queueMember == null
            ? null
            : _mapper.Map<QueueMember>(queueMember);
    }

    public async Task<int> EnqueueUserAsync(long groupId, int queueId, EnqueueUserRequest request, int? position, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();
        
        var queue = await enqueuerContext.Queues
            .Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.GroupId == groupId && q.Id == queueId, cancellationToken);

        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        var existingUser = await enqueuerContext.Users.FindAsync(new object[] { request.User.Id }, cancellationToken);
        if (existingUser == null)
        {
            existingUser = _mapper.Map<Persistence.Models.User>(request.User);
            enqueuerContext.Users.Add(existingUser);
        }

        if (queue.Members.Any(m => m.UserId == request.User.Id))
        {
            throw new UserAlreadyParticipatesException($"User with the \"{request.User.Id}\" ID already participates in the \"{queue.Name}\" queue.");
        }

        return position.HasValue
            ? await EnqueueOnPositionAsync(enqueuerContext, queue, existingUser, position.Value, cancellationToken)
            : await EnqueueAtFirstAvailablePositionAsync(enqueuerContext, queue, existingUser, cancellationToken);

        static async Task<int> EnqueueOnPositionAsync(EnqueuerContext enqueuerContext, Persistence.Models.Queue queue, Persistence.Models.User user, int position, CancellationToken cancellationToken)
        {
            queue.Members.Add(new Persistence.Models.QueueMember
            {
                Position = position,
                User = user,
                Queue = queue,
            });

            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return position;
        }

        static async Task<int> EnqueueAtFirstAvailablePositionAsync(EnqueuerContext enqueuerContext, Persistence.Models.Queue queue, Persistence.Models.User user, CancellationToken cancellationToken)
        {
            int firstAvailablePosition;
            try
            {
                firstAvailablePosition = await
                        (from position in enqueuerContext.Positions
                         join member in enqueuerContext.QueueMembers
                            on new { Position = position.Value, QueueId = queue.Id } equals new { member.Position, member.QueueId }
                            into joinedMembers
                         from member in joinedMembers.DefaultIfEmpty()
                         where member == null
                         select position.Value).FirstAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                throw new Exception /*QueueIsFullException*/($"All possible positions in the \"{queue.Name}\" queue are reserved.");
            }

            queue.Members.Add(new Persistence.Models.QueueMember
            {
                Position = firstAvailablePosition,
                User = user,
                Queue = queue,
            });

            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return firstAvailablePosition;
        }
    }

    public async Task DequeueUserAsync(long groupId, int queueId, long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queue = await enqueuerContext.Queues
            .Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.GroupId == groupId && q.Id == queueId, cancellationToken);

        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        var queueMember = queue.Members.FirstOrDefault(m => m.UserId == userId);
        if (queueMember == null)
        {
            throw new UserDoesNotParticipateException($"User with the \"{userId}\" ID does not participate in the queue with the \"{queueId}\" ID.");
        }

        queue.Members.Remove(queueMember);
        await enqueuerContext.SaveChangesAsync(cancellationToken);
    }
}

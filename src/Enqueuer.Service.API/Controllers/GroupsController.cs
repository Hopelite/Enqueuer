using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages;
using Enqueuer.Service.Messages.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public GroupsController(IGroupService groupService, IQueueService queueService)
    {
        _groupService = groupService;
        _queueService = queueService;
    }

    /// <summary>
    /// Get a group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupInfo>> GetGroup(long id, CancellationToken cancellationToken)
    {
        var group = await _groupService.GetGroupAsync(id, cancellationToken);
        if (group == null)
        {
            return NotFound($"Group with the \"{id}\" ID does not exist.");
        }

        return group;
    }

    /// <summary>
    /// Add a new or update an existing Telegram <paramref name="group"/> with the specified <paramref name="id"/>.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<GroupInfo>> AddOrUpdateGroup(long id, Group group, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (group.Id != id)
        {
            ModelState.AddModelError(nameof(group.Id), "The group ID must match the one specified in the URL. Updating the group ID is unsupported.");
            return UnprocessableEntity(ModelState);
        }

        var result = await _groupService.AddOrUpdateGroupAsync(id, group, cancellationToken);
        if (result == PutAction.Created)
        {
            return CreatedAtAction(nameof(GetGroup), new { id }, null);
        }

        return NoContent();
    }

    /// <summary>
    /// Get a user with the specified <paramref name="userId"/>, who participates in a group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetGroupMember(long id, long userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _groupService.GetGroupMemberAsync(id, userId, cancellationToken);
            if (user == null)
            {
                return NotFound($"User with the \"{userId}\" ID does not participate in the group with the \"{id}\" ID.");
            }

            return user;
        }
        catch (GroupDoesNotExistException)
        {
            return NotFound($"Group with the \"{id}\" ID does not exist.");
        }
    }

    /// <summary>
    /// Add a <paramref name="user"/> to a group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpPut("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AddOrUpdateGroupMember(long id, long userId, User user, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (user.Id != id)
        {
            ModelState.AddModelError(nameof(user.Id), "The user ID must match the one specified in the URL.");
            return UnprocessableEntity(ModelState);
        }

        try
        {
            var result = await _groupService.AddOrUpdateGroupMemberAsync(id, userId, user, cancellationToken);
            if (result == PutAction.Created)
            {
                return CreatedAtAction(nameof(GetGroupMember), new { id, userId }, null);
            }

            return NoContent();
        }
        catch (GroupDoesNotExistException)
        {
            return NotFound($"Group with the \"{id}\" ID does not exist.");
        }
    }

    /// <summary>
    /// Get a queue with the specified <paramref name="queueId"/> from a group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}/queues/{queueId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QueueInfo>> GetQueue(long id, int queueId, CancellationToken cancellationToken)
    {
        var queue = await _queueService.GetQueueAsync(id, queueId, cancellationToken);
        if (queue == null)
        {
            return NotFound($"Queue with the \"{queueId}\" ID does not exist in a group with the \"{id}\" ID.");
        }

        return queue;
    }

    /// <summary>
    /// Create a new queue with the specified name in the specified group.
    /// </summary>
    [HttpPost("{id}/queues")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateQueue(long id, CreateQueueRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        try
        {
            var queue = await _queueService.CreateQueueAsync(id, request, cancellationToken);
            return CreatedAtAction(nameof(GetQueue), new { id = queue.Id }, null);
        }
        catch (UserDoesNotExistException)
        {
            ModelState.AddModelError(nameof(request.CreatorId), $"User with the \"{request.CreatorId}\" ID does not exist.");
            return UnprocessableEntity(ModelState);
        }
        catch (GroupDoesNotExistException)
        {
            return NotFound($"Group with the \"{id}\" ID does not exist.");
        }
        catch (QueueAlreadyExistsException)
        {
            ModelState.AddModelError(nameof(request.QueueName), $"Queue \"{request.QueueName}\" already exists in the group with the \"{id}\" ID.");
            return Conflict(ModelState);
        }
    }

    /// <summary>
    /// Delete a queue with the specified <paramref name="id"/>.
    /// </summary>
    [HttpDelete("{id}/queues/{queueId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteQueue(long id, int queueId, CancellationToken cancellationToken)
    {
        try
        {
            await _queueService.DeleteQueueAsync(id, queueId, cancellationToken);
            return NoContent();
        }
        catch (QueueDoesNotExistException)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist in a group with the \"{id}\" ID.");
        }
    }

    /// <summary>
    /// Get a participant info with the specified <paramref name="userId"/> which participates in a queue with the specified <paramref name="queueId"/>
    /// in a group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}/queues/{queueId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QueueMember>> GetQueueMember(long id, int queueId, long userId, CancellationToken cancellationToken)
    {
        var queueMember = await _queueService.GetQueueMemberAsync(id, queueId, userId, cancellationToken);
        if (queueMember == null)
        {
            return NotFound($"User with the \"{userId}\" ID does not exist in the queue with the \"{queueId}\" ID in a group with the \"{id}\" ID.");
        }

        return queueMember;
    }

    /// <summary>
    /// Add user to a queue with the specified <paramref name="queueId"/> in a group with the specified <paramref name="id"/>.
    /// If position is specified, add user on it; otherwise adds on the first available position.
    /// </summary>
    [HttpPost("{id}/queues/{queueId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<int>> EnqueueUser(long id, int queueId, long userId, EnqueueUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (request.User.Id != userId)
        {
            ModelState.AddModelError(nameof(request.User.Id), "The user ID must match the one specified in the URL.");
            return UnprocessableEntity(ModelState);
        }

        if (request.Position.HasValue && request.Position.Value <= 0)
        {
            ModelState.AddModelError(nameof(request.Position), "The position must be a positive number.");
            return BadRequest(ModelState);
        }

        try
        {
            var position = await _queueService.EnqueueUserAsync(id, queueId, request, request.Position, cancellationToken);
            return CreatedAtAction(nameof(GetQueueMember), new { id, userId }, position);
        }
        catch (QueueDoesNotExistException)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist.");
        }
        catch (UserAlreadyParticipatesException)
        {
            return Conflict($"User with the \"{request.User.Id}\" ID already participates in the queue with the \"{id}\" ID.");
        }
    }

    /// <summary>
    /// Remove Telegram user with the specified <paramref name="userId"/> from a queue with the specified <paramref name="id"/>.
    /// </summary>
    [HttpDelete("{id}/queues/{queueId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DequeueUser(int id, int queueId, long userId, CancellationToken cancellationToken)
    {
        try
        {
            await _queueService.DequeueUserAsync(id, queueId, userId, cancellationToken);
            return Ok();
        }
        catch (QueueDoesNotExistException)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist.");
        }
        catch (UserDoesNotParticipateException)
        {
            return NotFound($"User with the \"{userId}\" ID does not participate in the queue with the \"{id}\" ID.");
        }
    }
}

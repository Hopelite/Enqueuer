using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.Messages;
using Enqueuer.Service.Messages.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueuesController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueuesController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    /// <summary>
    /// Get a queue with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QueueInfo>> GetQueue(int id, CancellationToken cancellationToken)
    {
        var queue = await _queueService.GetQueueAsync(id, cancellationToken);
        if (queue == null)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist.");
        }

        return queue;
    }

    /// <summary>
    /// Create a new queue with the specified name in the specified group.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QueueInfo>> CreateQueue(CreateQueueRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        try
        {
            var queue = await _queueService.CreateQueueAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetQueue), new { id = queue.Id }, queue);
        }
        catch (QueueAlreadyExistsException)
        {
            ModelState.AddModelError(nameof(request.QueueName), $"Queue \"{request.QueueName}\" already exists in the group with the \"{request.GroupId}\" ID.");
            return Conflict(ModelState);
        }
    }

    /// <summary>
    /// Delete a queue with the specified <paramref name="id"/>.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteQueue(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _queueService.DeleteQueueAsync(id, cancellationToken);
            return NoContent();
        }
        catch (QueueDoesNotExistException)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist.");
        }
    }

    /// <summary>
    /// Get a participant info with the specified <paramref name="userId"/> which participates in a queue with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QueueMember>> GetQueueMember(int id, long userId, CancellationToken cancellationToken)
    {
        var queueMember = await _queueService.GetQueueMemberAsync(id, userId, cancellationToken);
        if (queueMember == null)
        {
            return NotFound($"User with the \"{userId}\" ID does not exist in the queue with the \"{id}\" ID.");
        }

        return queueMember;
    }

    /// <summary>
    /// Add <paramref name="user"/> to a queue with the specified <paramref name="id"/>.
    /// If <paramref name="position"/> is specified, add user on it; otherwise adds on the first available position.
    /// </summary>
    [HttpPost("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> EnqueueUser(int id, long userId, int? position, User user, CancellationToken cancellationToken)
    {
        if (user.Id != userId)
        {
            ModelState.AddModelError(nameof(user.Id), "The user ID must match the one specified in the URL.");
            return UnprocessableEntity(ModelState);
        }

        if (position.HasValue && position.Value <= 0)
        {
            ModelState.AddModelError(nameof(position), "The position must be a positive number.");
            return BadRequest(ModelState);
        }

        try
        {
            position = await _queueService.EnqueueUserAsync(id, user, position, cancellationToken);
            return CreatedAtAction(nameof(GetQueueMember), new { id, userId }, position);
        }
        catch (QueueDoesNotExistException)
        {
            return NotFound($"Queue with the \"{id}\" ID does not exist.");
        }
        catch (UserAlreadyParticipatesException)
        {
            return Conflict($"User with the \"{user.Id}\" ID already participates in the queue with the \"{id}\" ID.");
        }
    }
}

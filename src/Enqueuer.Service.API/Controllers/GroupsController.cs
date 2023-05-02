using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
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
}

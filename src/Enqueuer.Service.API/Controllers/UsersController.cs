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
public class UsersController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;

    public UsersController(IGroupService groupService, IUserService userService)
    {
        _groupService = groupService;
        _userService = userService;
    }

    /// <summary>
    /// Get a user with the specified <paramref name="id"/>.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfo>> GetUser(long id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound($"User with the \"{id}\" ID does not exist.");
        }

        return user;
    }

    /// <summary>
    /// Add a new or update an existing Telegram <paramref name="user"/> with the specified <paramref name="id"/>.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AddOrUpdateUser(long id, User user, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (user.Id != id)
        {
            ModelState.AddModelError(nameof(user.Id), "The user ID must match the one specified in the URL. Updating the user ID is unsupported.");
            return UnprocessableEntity(ModelState);
        }

        var result = await _userService.AddOrUpdateUserAsync(id, user, cancellationToken);
        if (result == PutAction.Created)
        {
            return CreatedAtAction(nameof(GetUser), new { id }, null);
        }

        return NoContent();
    }

    /// <summary>
    /// Get all groups in which user with the specified <paramref name="id"/> participates.
    /// </summary>
    [HttpGet("{id}/groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Group[]>> GetUserGroups(long id, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: move to user service
            return await _groupService.GetUserGroupsAsync(id, cancellationToken);
        }
        catch (UserDoesNotExistException)
        {
            return NotFound($"User with the \"{id}\" ID does not exist.");
        }
    }
}

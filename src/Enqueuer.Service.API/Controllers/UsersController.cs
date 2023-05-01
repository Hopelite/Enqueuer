using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.Messages.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IGroupService _groupService;

    public UsersController(IGroupService groupService)
    {
        _groupService = groupService;
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
            return await _groupService.GetUserGroupsAsync(id, cancellationToken);
        }
        catch (UserDoesNotExistException)
        {
            return NotFound($"User with the \"{id}\" ID does not exist.");
        }
    }
}

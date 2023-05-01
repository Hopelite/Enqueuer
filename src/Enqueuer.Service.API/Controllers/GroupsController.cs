using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services;
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
    /// Add a new or update an existing Telegram group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

        return await _groupService.AddOrUpdateAsync(group, cancellationToken);
    }
}

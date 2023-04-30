using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Service.Messages.Models;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public UsersController(IGroupService groupService, IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all groups in which user with the specified <paramref name="id"/> participates.
    /// </summary>
    [HttpGet("{id}/groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Group[]>> GetUserGroups(long id, CancellationToken cancellationToken)
    {
        try
        {
            var groups = await _groupService.GetUserGroupsAsync(id, cancellationToken);
            return _mapper.Map<Group[]>(groups);
        }
        catch (UserDoesNotExistException)
        {
            return NotFound($"User with the \"{id}\" ID does not exist.");
        }
    }
}

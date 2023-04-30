using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Service.Messages.Models;
using Enqueuer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public GroupsController(IGroupService groupService, IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Adds new or updates an existing Telegram group with the specified <paramref name="id"/>.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Group>> AddOrUpdateGroup(long id, Group group, CancellationToken cancellationToken)
    {
        if (group.Id != id)
        {
            ModelState.AddModelError("Id", "The group ID must match the one specified in the URL. Updating the group ID is unsupported.");
            return UnprocessableEntity(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        //await _groupService.AddOrUpdateGroupAsync(, cancellationToken);

        throw new NotImplementedException();
    }
}

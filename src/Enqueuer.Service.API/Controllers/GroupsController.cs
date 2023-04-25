using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Service.Messages.Groups;
using Enqueuer.Service.Messages.Models;
using Enqueuer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Enqueuer.Service.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public GroupsController(IGroupService groupService, IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<GetUserGroupsResponse> GetUserGroups(GetUserGroupsRequest request, CancellationToken cancellationToken)
    {
        var groups = await _groupService.GetUserGroupsAsync(request.UserId, cancellationToken);

        return new GetUserGroupsResponse()
        {
            Groups = _mapper.Map<Group[]>(groups)
        };
    }
}

using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Responses;

public record GetUserGroupResponse(Group Group, User User);

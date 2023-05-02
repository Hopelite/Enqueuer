using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.API.Services;

public interface IUserService
{
    Task<UserInfo?> GetUserAsync(long userId, CancellationToken cancellationToken);

    Task<PutActionResponse> AddOrUpdateUserAsync(long userId, User user, CancellationToken cancellationToken);
}

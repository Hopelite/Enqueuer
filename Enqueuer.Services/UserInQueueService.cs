using System.Linq;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Services
{
    /// <inheritdoc/>
    public class UserInQueueService : IUserInQueueService
    {
        private readonly IRepository<UserInQueue> userInQueueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInQueueService"/> class.
        /// </summary>
        /// <param name="userInQueueRepository"><see cref="IRepository{T}"/> with <see cref="UserInQueue"/> entities.</param>
        public UserInQueueService(IRepository<UserInQueue> userInQueueRepository)
        {
            this.userInQueueRepository = userInQueueRepository;
        }

        /// <inheritdoc/>
        public int GetTotalUsersInQueue(Queue queue)
        {
            return this.userInQueueRepository.GetAll()
                .Count(userInQueue => userInQueue.Queue.Id == queue.Id);
        }
    }
}

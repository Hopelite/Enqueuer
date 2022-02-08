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
        public int GetFirstAvailablePosition(Queue queue)
        {
            var positions = this.userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .Select(userInQueue => userInQueue.Position)
                .OrderBy(position => position)
                .ToList();

            int firstAvailablePosition = 1;
            for (int i = 0; i < positions.Count; i++)
            {
                if (firstAvailablePosition != positions[i])
                {
                    break;
                }

                firstAvailablePosition++;
            }

            return firstAvailablePosition;
        }

        /// <inheritdoc/>
        public bool IsPositionReserved(Queue queue, int position)
        {
            return this.userInQueueRepository.GetAll()
                .Any(userInQueue => userInQueue.QueueId == queue.Id
                                 && userInQueue.Position == position);
        }
    }
}

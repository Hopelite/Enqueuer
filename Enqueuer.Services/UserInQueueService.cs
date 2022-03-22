using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Services
{
    /// <inheritdoc/>
    public class UserInQueueService : IUserInQueueService
    {
        private const int NumberOfPositions = 20;
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
        public IEnumerable<int> GetAvailablePositions(Queue queue)
        {
            var maxPosition = this.GetMaxPositionInQueue(queue);
            var availablePositions = this.GetAvailablePositions(queue, maxPosition);

            if (availablePositions.Count < NumberOfPositions)
            {
                for (int i = maxPosition + 1; availablePositions.Count < NumberOfPositions; i++)
                {
                    availablePositions.Add(i);
                }
            }

            return availablePositions;
        }

        private int GetMaxPositionInQueue(Queue queue)
        {
            int? maxPosition = this.userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .Select(userInQueue => userInQueue.Position)
                .DefaultIfEmpty()
                .Max(userInQueue => userInQueue);

            return maxPosition ?? NumberOfPositions;
        }

        private List<int> GetAvailablePositions(Queue queue, int maxPosition)
        {
            // TODO: optimize the algorithm
            var positionsToSearchIn = Enumerable.Range(1, maxPosition);
            var positionsReserved = this.userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .OrderBy(userInQueue => userInQueue.Position)
                .Select(userInQueue => userInQueue.Position);

            return positionsToSearchIn
                .Except(positionsReserved)
                .Take(NumberOfPositions).ToList();
        }

        /// <inheritdoc/>
        public int GetFirstAvailablePosition(Queue queue)
        {
            var positions = this.userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .Select(userInQueue => userInQueue.Position)
                .OrderBy(position => position)
                .ToList();

            var firstAvailablePosition = 1;
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

        /// <inheritdoc/>
        public async Task AddUserToQueue(User user, Queue queue, int position)
        {
            var userInQueue = new UserInQueue()
            {
                Position = position,
                UserId = user.Id,
                QueueId = queue.Id,
            };

            await this.userInQueueRepository.AddAsync(userInQueue);
        }
    }
}

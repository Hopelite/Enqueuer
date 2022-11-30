using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Services
{
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

        public IEnumerable<int> GetAvailablePositions(Queue queue)
        {
            var maxPosition = GetMaxPositionInQueue(queue);
            var availablePositions = GetAvailablePositions(queue, maxPosition);

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
            int? maxPosition = userInQueueRepository.GetAll()
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
            var positionsReserved = userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .OrderBy(userInQueue => userInQueue.Position)
                .Select(userInQueue => userInQueue.Position);

            return positionsToSearchIn
                .Except(positionsReserved)
                .Take(NumberOfPositions).ToList();
        }

        public int GetFirstAvailablePosition(Queue queue)
        {
            // TODO: optimize the algorithm
            var positions = userInQueueRepository.GetAll()
                .Where(userInQueue => userInQueue.QueueId == queue.Id)
                .Select(userInQueue => userInQueue.Position)
                .OrderBy(position => position)
                .ToList();

            var firstAvailablePosition = 1;
            while (positions.Contains(firstAvailablePosition))
            {
                firstAvailablePosition++;
            }

            return firstAvailablePosition;
        }

        public bool IsPositionReserved(Queue queue, int position)
        {
            return userInQueueRepository.GetAll()
                .Any(userInQueue => userInQueue.QueueId == queue.Id
                                 && userInQueue.Position == position);
        }

        public async Task AddUserToQueueAsync(User user, Queue queue, int position)
        {
            var userInQueue = new UserInQueue()
            {
                Position = position,
                UserId = user.Id,
                QueueId = queue.Id,
            };

            await userInQueueRepository.AddAsync(userInQueue);
        }

        public async Task CompressQueuePositionsAsync(Queue queue, int startingAtPosition = 1)
        {
            var usersInQueue = userInQueueRepository.GetAll()
                .Where(userInQueue
                    => userInQueue.QueueId == queue.Id
                    && userInQueue.Position >= startingAtPosition)
                .OrderBy(userInQueue => userInQueue.Position)
                .ToList();

            var currentPosition = startingAtPosition;
            foreach (var userInQueue in usersInQueue)
            {
                if (userInQueue.Position != currentPosition)
                {
                    userInQueue.Position = currentPosition;
                    await userInQueueRepository.UpdateAsync(userInQueue);
                }

                currentPosition++;
            }
        }
    }
}

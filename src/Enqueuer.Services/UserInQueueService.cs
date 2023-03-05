using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services
{
    public class UserInQueueService : IUserInQueueService
    {
        private const int NumberOfPositions = 20;
        private readonly EnqueuerContext _enqueuerContext;

        public UserInQueueService(EnqueuerContext enqueuerContext)
        {
            _enqueuerContext = enqueuerContext;
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
            throw new NotImplementedException();
            //int? maxPosition = userInQueueRepository.GetAll()
            //    .Where(userInQueue => userInQueue.QueueId == queue.Id)
            //    .Select(userInQueue => userInQueue.Position)
            //    .DefaultIfEmpty()
            //    .Max(userInQueue => userInQueue);

            //return maxPosition ?? NumberOfPositions;
        }

        private List<int> GetAvailablePositions(Queue queue, int maxPosition)
        {
            throw new NotImplementedException();
            // TODO: optimize the algorithm
            //var positionsToSearchIn = Enumerable.Range(1, maxPosition);
            //var positionsReserved = userInQueueRepository.GetAll()
            //    .Where(userInQueue => userInQueue.QueueId == queue.Id)
            //    .OrderBy(userInQueue => userInQueue.Position)
            //    .Select(userInQueue => userInQueue.Position);

            //return positionsToSearchIn
            //    .Except(positionsReserved)
            //    .Take(NumberOfPositions).ToList();
        }

        public int GetFirstAvailablePosition(Queue queue)
        {
            throw new NotImplementedException();
            // TODO: optimize the algorithm
            //var positions = userInQueueRepository.GetAll()
            //    .Where(userInQueue => userInQueue.QueueId == queue.Id)
            //    .Select(userInQueue => userInQueue.Position)
            //    .OrderBy(position => position)
            //    .ToList();

            //var firstAvailablePosition = 1;
            //while (positions.Contains(firstAvailablePosition))
            //{
            //    firstAvailablePosition++;
            //}

            //return firstAvailablePosition;
        }

        public bool IsPositionReserved(Queue queue, int position)
        {
            throw new NotImplementedException();
            //return userInQueueRepository.GetAll()
            //    .Any(userInQueue => userInQueue.QueueId == queue.Id
            //                     && userInQueue.Position == position);
        }

        public Task AddUserToQueueAsync(User user, Queue queue, int position)
        {
            throw new NotImplementedException();
            //var userInQueue = new QueueMember()
            //{
            //    Position = position,
            //    UserId = user.Id,
            //    QueueId = queue.Id,
            //};

            //return userInQueueRepository.AddAsync(userInQueue);
        }

        public async Task CompressQueuePositionsAsync(Queue queue, int startingAtPosition = 1)
        {
            throw new NotImplementedException();
            //var usersInQueue = userInQueueRepository.GetAll()
            //    .Where(userInQueue
            //        => userInQueue.QueueId == queue.Id
            //        && userInQueue.Position >= startingAtPosition)
            //    .OrderBy(userInQueue => userInQueue.Position)
            //    .ToList();

            //var currentPosition = startingAtPosition;
            //foreach (var userInQueue in usersInQueue)
            //{
            //    if (userInQueue.Position != currentPosition)
            //    {
            //        userInQueue.Position = currentPosition;
            //        await userInQueueRepository.UpdateAsync(userInQueue);
            //    }

            //    currentPosition++;
            //}
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enqueuer.Services.Tests
{
    [TestFixture]
    public class UserInQueueServiceTests
    {
        private const int NumberOfPositions = 20;
        private readonly Mock<IRepository<UserInQueue>> _userInQueueRepositoryMock;
        private readonly IUserInQueueService _userInQueueService;

        public UserInQueueServiceTests()
        {
            _userInQueueRepositoryMock = new Mock<IRepository<UserInQueue>>();
            _userInQueueService = new UserInQueueService(_userInQueueRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            _userInQueueRepositoryMock.Reset();
        }

        [Test]
        public void GetFirstAvailablePosition_GetsFirstAvailablePosition()
        {
            // Arrange
            const int expected = 3;
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 1 },
                new UserInQueue() { Position = 2, QueueId = 1 },
                new UserInQueue() { Position = 4, QueueId = 1 },
                new UserInQueue() { Position = 6, QueueId = 1 },
                new UserInQueue() { Position = 7, QueueId = 1 },
            };

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetFirstAvailablePosition(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetFirstAvailablePosition_QueueHasNoParticipants_ReturnsFirstPosition()
        {
            // Arrange
            const int expected = 1;
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 3 },
                new UserInQueue() { Position = 2, QueueId = 2 },
                new UserInQueue() { Position = 4, QueueId = 4 },
                new UserInQueue() { Position = 6, QueueId = 2 },
                new UserInQueue() { Position = 7, QueueId = 3 },
            };

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetFirstAvailablePosition(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void IsPositionReserved_PositionIsAvaliable()
        {
            // Arrange
            const int position = 3;
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 1 },
                new UserInQueue() { Position = 2, QueueId = 1 },
                new UserInQueue() { Position = 4, QueueId = 1 },
                new UserInQueue() { Position = 6, QueueId = 1 },
                new UserInQueue() { Position = 7, QueueId = 1 },
            };

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.IsPositionReserved(queue, position);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsPositionReserved_PositionIsReserved()
        {
            // Arrange
            const int position = 1;
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 1 },
                new UserInQueue() { Position = 2, QueueId = 1 },
                new UserInQueue() { Position = 4, QueueId = 1 },
                new UserInQueue() { Position = 6, QueueId = 1 },
                new UserInQueue() { Position = 7, QueueId = 1 },
            };

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.IsPositionReserved(queue, position);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void GetAvailablePositions_AllPositionsAvailable()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = Enumerable.Empty<UserInQueue>();
            var expected = Enumerable.Range(1, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetAvailablePositions_FirstTwentyPositionsReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = GenerateUsersInQueue(NumberOfPositions, queue.Id);
            var expected = Enumerable.Range(NumberOfPositions + 1, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetAvailablePositions_FirstFortyPositionsReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = GenerateUsersInQueue(NumberOfPositions * 2, queue.Id);
            var expected = Enumerable.Range(NumberOfPositions * 2 + 1, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetAvailablePositions_SomePositionsInFirstTwentyReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new UserInQueue[]
            {
                new UserInQueue() { Position = 4, QueueId = queue.Id },
                new UserInQueue() { Position = 7, QueueId = queue.Id },
            };

            var expected = GenerateAvailablePositions(usersInQueues, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetAvailablePositions_AllPositionsInFirstTwentyAndSomeInTheNextAreReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var extraReservedPositions = new UserInQueue[]
            {
                new UserInQueue() { Position = 22, QueueId = queue.Id },
                new UserInQueue() { Position = 34, QueueId = queue.Id },
            };

            var usersInQueues = GenerateUsersInQueue(NumberOfPositions, queue.Id).ToList();
            usersInQueues.AddRange(extraReservedPositions);

            var expected = GenerateAvailablePositions(usersInQueues, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetAvailablePositions_SomePositionsInFirstTwentyAndSomeInTheNextAreReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var reservedPositions = new UserInQueue[]
            {
                new UserInQueue() { Position = 1, QueueId = queue.Id },
                new UserInQueue() { Position = 4, QueueId = queue.Id },
                new UserInQueue() { Position = 5, QueueId = queue.Id },
                new UserInQueue() { Position = 11, QueueId = queue.Id },
                new UserInQueue() { Position = 22, QueueId = queue.Id },
                new UserInQueue() { Position = 34, QueueId = queue.Id },
            };

            var expected = GenerateAvailablePositions(reservedPositions, NumberOfPositions);

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(reservedPositions.AsQueryable());

            // Act
            var actual = _userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task AddUserToQueue_AddsUserToQueue()
        {
            // Arrange
            const int position = 1;
            var queue = new Queue() { Id = 1 };
            var user = new User() { Id = 1 };

            _userInQueueRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<UserInQueue>()));

            // Act
            await _userInQueueService.AddUserToQueueAsync(user, queue, position);

            // Assert
            _userInQueueRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<UserInQueue>()), Times.Once());
        }

        [Test]
        public async Task CompressQueuePositionsAsync_CompressesPositions()
        {
            // Arrange
            const int numberOfUsers = 25;

            var queue = new Queue() { Id = 1 };
            var usersInQueue = GenerateUsersInQueueRandomPositions(numberOfUsers, queue.Id).ToList();

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueue.AsQueryable());

            // Act
            await _userInQueueService.CompressQueuePositionsAsync(queue);

            // Assert
            AssertAreCompressed(usersInQueue);
        }

        [Test]
        public async Task CompressQueuePositionsAsync_StartingFromPosition_CompressesPositionsStartingFromSpecifiedOne()
        {
            // Arrange
            const int startingPosition = 10;
            var queue = new Queue() { Id = 1 };
            var usersInQueue = new UserInQueue[]
            {
                new UserInQueue() { Position = 4, QueueId = 1 },
                new UserInQueue() { Position = 8, QueueId = 1 },
                new UserInQueue() { Position = 6, QueueId = 1 },
                new UserInQueue() { Position = startingPosition, QueueId = 1 },
                new UserInQueue() { Position = 14, QueueId = 1 },
                new UserInQueue() { Position = 19, QueueId = 1 },
                new UserInQueue() { Position = 34, QueueId = 1 },
            };

            var expectedUncompressedUsers = usersInQueue.Where(u => u.Position < startingPosition).ToArray();

            _userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueue.AsQueryable());

            // Act
            await _userInQueueService.CompressQueuePositionsAsync(queue, startingPosition);

            // Assert
            for (int i = 0; i < expectedUncompressedUsers.Length; i++)
            {
                Assert.AreEqual(expectedUncompressedUsers[i].Position, usersInQueue[i].Position);
            }

            var expectedPosition = startingPosition;
            for (int i = expectedUncompressedUsers.Length; i < usersInQueue.Length; i++)
            {
                Assert.AreEqual(expectedPosition, usersInQueue[i].Position);
                expectedPosition++;
            }
        }

        private static List<int> GenerateAvailablePositions(IEnumerable<UserInQueue> usersInQueues, int positionsToGet)
        {
            var result = new List<int>(NumberOfPositions);
            int startPosition = 1;
            foreach (var user in usersInQueues.OrderBy(userInQueue => userInQueue.Position))
            {
                result.AddRange(Enumerable.Range(startPosition, user.Position - startPosition));
                startPosition = user.Position + 1;
            }

            if (result.Count < NumberOfPositions)
            {
                result.AddRange(Enumerable.Range(startPosition, NumberOfPositions - result.Count));
            }

            return result.Take(positionsToGet).ToList();
        }

        private static IEnumerable<UserInQueue> GenerateUsersInQueueRandomPositions(int numberOfUsers, int queueId)
        {
            var random = new Random();
            var usersInQueue = new UserInQueue[numberOfUsers];
            for (int i = 0; i < numberOfUsers; i++)
            {
                int position = 0;
                do
                {
                    position = random.Next(1, int.MaxValue);
                } 
                while (usersInQueue.Any(u => u != null && u.Position == position));

                usersInQueue[i] = new UserInQueue() { Position = position, QueueId = queueId };
            }

            return usersInQueue;
        }

        private static IEnumerable<UserInQueue> GenerateUsersInQueue(int numberOfUsers, int queueId)
        {
            var usersInQueue = new UserInQueue[numberOfUsers];
            for (int i = 0; i < numberOfUsers; i++)
            {
                usersInQueue[i] = new UserInQueue() { Position = i + 1, QueueId = queueId };
            }

            return usersInQueue;
        }

        private static void AssertAreCompressed(IEnumerable<UserInQueue> usersInQueue)
        {
            var expectedPosition = 1;
            foreach (var user in usersInQueue.OrderBy(u => u.Position))
            {
                Assert.AreEqual(expectedPosition, user.Position);
                expectedPosition++;
            }
        }
    }
}

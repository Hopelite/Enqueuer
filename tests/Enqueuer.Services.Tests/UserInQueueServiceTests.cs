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
        private readonly Mock<IRepository<UserInQueue>> userInQueueRepositoryMock;
        private readonly IUserInQueueService userInQueueService;

        public UserInQueueServiceTests()
        {
            this.userInQueueRepositoryMock = new Mock<IRepository<UserInQueue>>();
            this.userInQueueService = new UserInQueueService(userInQueueRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            this.userInQueueRepositoryMock.Reset();
        }

        [Test]
        public void UserInQueueServiceTests_GetFirstAvailablePosition()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 1 },
                new UserInQueue() { Position = 2, QueueId = 1 },
                new UserInQueue() { Position = 4, QueueId = 1 },
                new UserInQueue() { Position = 6, QueueId = 1 },
                new UserInQueue() { Position = 7, QueueId = 1 },
            };
            const int expected = 3;

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetFirstAvailablePosition(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetFirstAvailablePosition_QueueHasNoParticipants()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new List<UserInQueue>()
            {
                new UserInQueue() { Position = 1, QueueId = 3 },
                new UserInQueue() { Position = 2, QueueId = 2 },
                new UserInQueue() { Position = 4, QueueId = 4 },
                new UserInQueue() { Position = 6, QueueId = 2 },
                new UserInQueue() { Position = 7, QueueId = 3 },
            };
            const int expected = 1;

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetFirstAvailablePosition(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_IsPositionReserved_PositionIsAvaliable()
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

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.IsPositionReserved(queue, position);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void UserInQueueServiceTests_IsPositionReserved_PositionIsReserved()
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

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.IsPositionReserved(queue, position);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_AllPositionsAvailable()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = Enumerable.Empty<UserInQueue>();
            var expected = Enumerable.Range(1, NumberOfPositions);

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_FirstTwentyPositionsReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = GenerateUsersInQueue(NumberOfPositions, queue.Id);
            var expected = Enumerable.Range(NumberOfPositions + 1, NumberOfPositions);

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_FirstFortyPositionsReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = GenerateUsersInQueue(NumberOfPositions * 2, queue.Id);
            var expected = Enumerable.Range(NumberOfPositions * 2 + 1, NumberOfPositions);

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_SomePositionsInFirstTwentyReserved()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var usersInQueues = new UserInQueue[]
            {
                new UserInQueue() { Position = 4, QueueId = queue.Id },
                new UserInQueue() { Position = 7, QueueId = queue.Id },
            };

            var expected = GenerateAvailablePositions(usersInQueues, NumberOfPositions);

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_AllPositionsInFirstTwentyAndSomeInTheNextAreReserved()
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

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(usersInQueues.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UserInQueueServiceTests_GetAvailablePositions_SomePositionsInFirstTwentyAndSomeInTheNextAreReserved()
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

            this.userInQueueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(reservedPositions.AsQueryable());

            // Act
            var actual = this.userInQueueService.GetAvailablePositions(queue);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task UserInQueueServiceTests_AddUserToQueue_AddsUserToQueue()
        {
            // Arrange
            var queue = new Queue() { Id = 1 };
            var user = new User() { Id = 1 };
            const int position = 1;

            this.userInQueueRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<UserInQueue>()));

            // Act
            await this.userInQueueService.AddUserToQueue(user, queue, position);

            // Assert
            this.userInQueueRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<UserInQueue>()), Times.Once());
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

        private static IEnumerable<UserInQueue> GenerateUsersInQueue(int numberOfUsers, int queueId)
        {
            var usersInQueue = new UserInQueue[numberOfUsers];
            for (int i = 0; i < numberOfUsers; i++)
            {
                usersInQueue[i] = new UserInQueue() { Position = i + 1, QueueId = queueId };
            }

            return usersInQueue;
        }
    }
}

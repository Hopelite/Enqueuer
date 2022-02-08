using System.Collections.Generic;
using System.Linq;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class UserInQueueServiceTests
    {
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
    }
}

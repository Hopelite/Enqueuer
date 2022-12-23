using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enqueuer.Services.Tests
{
    [TestFixture]
    public class QueueExtensionsTests
    {
        [Test]
        public void TryGetUserPosition_GetsUserPosition()
        {
            // Arrange
            var expectedPosition = 10;
            var user = new User() { Id = 5 };

            var queue = new Queue()
            {
                Users = new List<UserInQueue>
                {
                    new UserInQueue() { UserId = 1, Position = 14 },
                    new UserInQueue() { UserId = 2, Position = 114 },
                    new UserInQueue() { UserId = 3, Position = 4 },
                    new UserInQueue() { UserId = 4, Position = 13 },
                    new UserInQueue() { UserId = 5, Position = expectedPosition },
                    new UserInQueue() { UserId = 6, Position = 27 },
                }
            };

            // Act
            var result = queue.TryGetUserPosition(user, out var actualPosition);

            // Assert
            Assert.True(result);
            Assert.AreEqual(expectedPosition, actualPosition);
        }

        [Test]
        public void TryGetUserPosition_UserDoesNotParticipateInQueue_ReturnsFalse()
        {
            // Arrange
            var user = new User() { Id = 5 };
            var queue = new Queue()
            {
                Users = new List<UserInQueue>
                {
                    new UserInQueue() { UserId = 1, Position = 14 },
                    new UserInQueue() { UserId = 2, Position = 114 },
                    new UserInQueue() { UserId = 3, Position = 4 },
                    new UserInQueue() { UserId = 4, Position = 13 },
                    new UserInQueue() { UserId = 6, Position = 27 },
                }
            };

            // Act
            var result = queue.TryGetUserPosition(user, out var _);

            // Assert
            Assert.False(result);
        }
    }
}

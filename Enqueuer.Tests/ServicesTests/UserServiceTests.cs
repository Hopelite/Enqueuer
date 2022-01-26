using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Enqueuer.Tests.Utilities.Comparers;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class UserServiceTests : ServicesTestsBase
    {
        [Test]
        public async Task UserServiceTests_GetNewOrExistingUser_CreatesAndReturnNewUser()
        {
            // Arrange
            var comparer = new UserComparer();
            var expected = new Telegram.Bot.Types.User()
            {
                Id = 1,
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Username = "TestUsername"
            };

            var users = Enumerable.Empty<User>().AsQueryable();

            var mock = new Mock<IRepository<User>>();
            mock.Setup(repository => repository.GetAll())
                .Returns(users);
            mock.Setup(repository => repository.AddAsync(It.IsAny<User>()));
            var userService = new UserService(mock.Object);

            // Act
            var actual = await userService.GetNewOrExistingUser(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            mock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task UserServiceTests_GetNewOrExistingUser_ReturnExistingUser()
        {
            // Arrange
            var comparer = new UserComparer();
            var expected = new Telegram.Bot.Types.User()
            {
                Id = 1,
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Username = "TestUsername"
            };

            var users = new List<User> { expected }.AsQueryable();

            var mock = new Mock<IRepository<User>>();
            mock.Setup(repository => repository.GetAll())
                .Returns(users);
            mock.Setup(repository => repository.AddAsync(It.IsAny<User>()));
            var userService = new UserService(mock.Object);

            // Act
            var actual = await userService.GetNewOrExistingUser(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            mock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}

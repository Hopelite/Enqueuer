using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Tests.Utilities.Comparers;
using Moq;
using NUnit.Framework;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> userRepositoryMock;
        private readonly UserService userService;

        public UserServiceTests()
        {
            this.userRepositoryMock = new Mock<IRepository<User>>();
            this.userService = new UserService(userRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            this.userRepositoryMock.Reset();
        }

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

            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);
            this.userRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<User>()));

            // Act
            var actual = await this.userService.GetNewOrExistingUserAsync(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            this.userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
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

            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);
            this.userRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<User>()));

            // Act
            var actual = await this.userService.GetNewOrExistingUserAsync(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            this.userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}

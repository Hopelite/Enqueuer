using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Tests.Utilities.Comparers;
using Enqueuer.Services.Tests.Utilities.Wrappers;
using Moq;
using NUnit.Framework;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services.Tests
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
        public async Task UserServiceTests_GetNewOrExistingUser_CreatesAndReturnsNewUser()
        {
            // Arrange
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
            this.userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task UserServiceTests_GetNewOrExistingUser_ReturnsExistingUser()
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

        [Test]
        public void UserServiceTests_GetUserByUserId_ReturnsExistingUser()
        {
            // Arrange
            const long expectedUserId = 2;
            const long expectedUserIndex = 1;
            var comparer = new UserComparer();
            var expected = new User[]
            {
                new User() { UserId = 1, FirstName = "TestFirstName", LastName = "TestLastName", },
                new User() { UserId = 2, FirstName = "TestSecondName", LastName = "TestLastName" },
                new User() { UserId = 3, FirstName = "TestThirdName", LastName = "TestLastName" },
            };

            var users = new List<User>(expected).AsQueryable();

            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);

            // Act
            var actual = this.userService.GetUserByUserId(expectedUserId);

            // Assert
            Assert.IsTrue(comparer.Equals(expected[expectedUserIndex], actual));
        }

        [Test]
        public void UserServiceTests_GetUserByUserId_ReturnsNull()
        {
            // Arrange
            const long expectedUserId = 100;
            var expected = new User[]
            {
                new User() { UserId = 1, FirstName = "TestFirstName", LastName = "TestLastName" },
                new User() { UserId = 2, FirstName = "TestSecondName", LastName = "TestLastName" },
                new User() { UserId = 3, FirstName = "TestThirdName", LastName = "TestLastName" },
            };

            var users = new List<User>(expected).AsQueryable();

            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);

            // Act
            var actual = this.userService.GetUserByUserId(expectedUserId);

            // Assert
            Assert.IsNull(actual);
        }

        [Test]
        public void UserServiceTests_GetUserChats_ReturnsUserChats()
        {
            // Arrange
            const long userId = 1;
            var comparer = new ChatComparer();
            var expected = new Chat[]
            {
                new Chat() { Id = 1 },
                new Chat() { Id = 2 },
                new Chat() { Id = 3 },
                new Chat() { Id = 4 },
                new Chat() { Id = 5 },
            };

            var user = new User()
            {
                UserId = userId,
                Chats = expected,
            };

            var users = new List<User>() { user }.AsQueryable();
            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);

            // Act
            var actual = this.userService.GetUserChats(userId);

            // Assert
            AssertWrapper.StrictMultipleEquals(expected, actual, comparer);
        }

        [Test]
        public void UserServiceTests_GetUserChats_UserHasNoChats_ReturnsEmptyEnumerable()
        {
            // Arrange
            const long userId = 1;
            var comparer = new ChatComparer();
            var expected = Enumerable.Empty<Chat>().ToList();

            var user = new User()
            {
                UserId = userId,
                Chats = expected,
            };

            var users = new List<User>() { user }.AsQueryable();
            this.userRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(users);

            // Act
            var actual = this.userService.GetUserChats(userId);

            // Assert
            Assert.IsEmpty(actual);
        }
    }
}

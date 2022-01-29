using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Tests.Utilities.Comparers;
using Moq;
using NUnit.Framework;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class ChatServiceTests
    {
        private readonly Mock<IRepository<Chat>> chatRepositoryMock;
        private readonly ChatService chatService;

        public ChatServiceTests()
        {
            this.chatRepositoryMock = new Mock<IRepository<Chat>>();
            this.chatService = new ChatService(chatRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            this.chatRepositoryMock.Reset();
        }

        [Test]
        public async Task ChatServiceTests_GetNewOrExistingChat_CreatesAndReturnNewChat()
        {
            // Arrange
            var comparer = new ChatComparer();
            var expected = new Telegram.Bot.Types.Chat()
            {
                Id = 1,
            };

            var chats = Enumerable.Empty<Chat>().AsQueryable();

            this.chatRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(chats);
            this.chatRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));

            // Act
            var actual = await this.chatService.GetNewOrExistingChatAsync(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            this.chatRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Once);
        }

        [Test]
        public async Task ChatServiceTests_GetNewOrExistingUser_CreatesAndReturnNewChat()
        {
            // Arrange
            var comparer = new ChatComparer();
            var expected = new Telegram.Bot.Types.Chat()
            {
                Id = 1
            };

            var chats = new List<Chat> { expected }.AsQueryable();

            this.chatRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(chats);
            this.chatRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));

            // Act
            var actual = await this.chatService.GetNewOrExistingChatAsync(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            this.chatRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Never);
        }
    }
}

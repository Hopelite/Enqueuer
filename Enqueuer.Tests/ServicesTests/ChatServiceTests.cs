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

            var mock = new Mock<IRepository<Chat>>();
            mock.Setup(repository => repository.GetAll())
                .Returns(chats);
            mock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));
            var chatService = new ChatService(mock.Object);

            // Act
            var actual = await chatService.GetNewOrExistingChat(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            mock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Once);
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

            var mock = new Mock<IRepository<Chat>>();
            mock.Setup(repository => repository.GetAll())
                .Returns(chats);
            mock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));
            var chatService = new ChatService(mock.Object);

            // Act
            var actual = await chatService.GetNewOrExistingChat(expected);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
            mock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Never);
        }
    }
}

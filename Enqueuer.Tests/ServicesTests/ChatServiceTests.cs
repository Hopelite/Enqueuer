using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class ChatServiceTests
    {
        private const string databaseName = "Test";
        private EnqueuerContext context;
        private IRepository<Chat> chatRepository;
        private IChatService chatService;

        public ChatServiceTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<EnqueuerContext>()
                .UseInMemoryDatabase(databaseName).Options;
            this.context = new EnqueuerContext(dbContextOptions);
            this.chatRepository = new Repository<Chat>(context);
            this.chatService = new ChatService(chatRepository);
        }

        [Test]
        public async Task ChatServiceTests_GetNewOrExistingChat_CreatesAndReturnNewChat()
        {
            // Arrange
            var expected = new Chat()
            {
                ChatId = 1,
            };

            // Act
            var actual = await this.chatService.GetNewOrExistingChat(expected.ChatId);

            // Asssert
            Assert.AreEqual(expected.ChatId, actual.ChatId);
        }
    }
}

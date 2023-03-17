// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Enqueuer.Persistence.Models;
// using Enqueuer.Persistence.Repositories;
// using Enqueuer.Services.Tests.Utilities.Comparers;
// using Moq;
// using NUnit.Framework;

// namespace Enqueuer.Services.Tests
// {
//     [TestFixture]
//     public class ChatServiceTests
//     {
//         private readonly Mock<IRepository<Chat>> chatRepositoryMock;
//         private readonly ChatService chatService;

//         public ChatServiceTests()
//         {
//             this.chatRepositoryMock = new Mock<IRepository<Chat>>();
//             this.chatService = new ChatService(chatRepositoryMock.Object);
//         }

//         [SetUp]
//         public void Setup()
//         {
//             this.chatRepositoryMock.Reset();
//         }

//         [Test]
//         public async Task ChatServiceTests_GetNewOrExistingChat_CreatesAndReturnsNewChat()
//         {
//             // Arrange
//             var expected = new Telegram.Bot.Types.Chat()
//             {
//                 Id = 1,
//             };

//             var chats = Enumerable.Empty<Chat>().AsQueryable();

//             this.chatRepositoryMock.Setup(repository => repository.GetAll())
//                 .Returns(chats);
//             this.chatRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));

//             // Act
//             var actual = await this.chatService.GetOrCreateChatAsync(expected);

//             // Assert
//             this.chatRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Once);
//         }

//         [Test]
//         public async Task ChatServiceTests_GetNewOrExistingUser_CreatesAndReturnsNewChat()
//         {
//             // Arrange
//             var comparer = new ChatComparer();
//             var expected = new Telegram.Bot.Types.Chat()
//             {
//                 Id = 1
//             };

//             var chats = new List<Chat> { expected }.AsQueryable();

//             this.chatRepositoryMock.Setup(repository => repository.GetAll())
//                 .Returns(chats);
//             this.chatRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<Chat>()));

//             // Act
//             var actual = await this.chatService.GetOrCreateChatAsync(expected);

//             // Assert
//             Assert.IsTrue(comparer.Equals(expected, actual));
//             this.chatRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Chat>()), Times.Never);
//         }

//         [Test]
//         public async Task ChatServiceTests_AddUserToChat_AddsUserToChat()
//         {
//             // Arrange
//             var chat = new Chat()
//             {
//                 Users = Enumerable.Empty<User>().ToList()
//             };

//             var user = new User()
//             {
//                 UserId = 1
//             };

//             this.chatRepositoryMock.Setup(repository => repository.UpdateAsync(It.IsAny<Chat>()));

//             // Act
//             await this.chatService.AddUserToChatIfNotAlready(user, chat);

//             // Assert
//             Assert.IsNotNull(chat.Users.FirstOrDefault(chatUser => chatUser.UserId == user.UserId));
//             this.chatRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Chat>()), Times.Once);
//         }

//         [Test]
//         public async Task ChatServiceTests_AddUserToChat_UserIsAlreadyInChat()
//         {
//             // Arrange
//             var user = new User()
//             {
//                 UserId = 1
//             };

//             var chat = new Chat()
//             {
//                 Users = new List<User>()
//                 {
//                     user
//                 }
//             };

//             this.chatRepositoryMock.Setup(repository => repository.UpdateAsync(It.IsAny<Chat>()));

//             // Act
//             await this.chatService.AddUserToChatIfNotAlready(user, chat);

//             // Assert
//             Assert.IsNotNull(chat.Users.FirstOrDefault(chatUser => chatUser.UserId == user.UserId));
//             this.chatRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Chat>()), Times.Never);
//         }

//         [Test]
//         public void ChatServiceTests_GetNumberOfQueues_ReturnsCount()
//         {
//             // Arrange
//             const long chatId = 1;
//             var queues = new[]
//             {
//                 new Queue(),
//                 new Queue(),
//                 new Queue()
//             };

//             var expected = queues.Length;
//             var chat = new Chat()
//             {
//                 ChatId = chatId,
//                 Queues = new List<Queue>(queues)
//             };

//             var chats = new List<Chat>() { chat }.AsQueryable();

//             this.chatRepositoryMock.Setup(repository => repository.GetAll())
//                 .Returns(chats);

//             // Act
//             var actual = this.chatService.GetNumberOfQueues(chat.ChatId);

//             // Assert
//             Assert.AreEqual(expected, actual);
//         }

//         [Test]
//         public void ChatServiceTests_GetChatByTelegramChatId_ReturnsChat()
//         {
//             // Arrange
//             const long chatId = 2;
//             var comparer = new ChatComparer();
//             var expected = new Chat() { ChatId = chatId };
//             var chats = new List<Chat>()
//             {
//                 new Chat() { ChatId = 1 },
//                 new Chat() { ChatId = 2 },
//                 new Chat() { ChatId = 3 },
//             }.AsQueryable();

//             this.chatRepositoryMock.Setup(repository => repository.GetAll())
//                 .Returns(chats);

//             // Act
//             var actual = this.chatService.GetChatByTelegramChatId(chatId);

//             // Assert
//             Assert.IsTrue(comparer.Equals(expected, actual));
//         }

//         [Test]
//         public void ChatServiceTests_GetChatByTelegramChatId_ChatDoesNotExist_ReturnsNull()
//         {
//             // Arrange
//             const long chatId = 1;
//             var chats = Enumerable.Empty<Chat>().AsQueryable();

//             this.chatRepositoryMock.Setup(repository => repository.GetAll())
//                 .Returns(chats);

//             // Act
//             var actual = this.chatService.GetChatByTelegramChatId(chatId);

//             // Assert
//             Assert.IsNull(actual);
//         }

//         [Test]
//         public void ChatServiceTests_GetChatById_ReturnsChat()
//         {
//             // Arrange
//             const int id = 2;
//             var comparer = new ChatComparer();
//             var expected = new Chat() { Id = id };

//             this.chatRepositoryMock.Setup(repository => repository.Get(id))
//                 .Returns(expected);

//             // Act
//             var actual = this.chatService.GetChatById(id);

//             // Assert
//             Assert.IsTrue(comparer.Equals(expected, actual));
//         }

//         [Test]
//         public void ChatServiceTests_GetChatById_ChatDoesNotExist_ReturnsNull()
//         {
//             // Arrange
//             const int id = 1;
//             Chat nullResult = null;
//             this.chatRepositoryMock.Setup(repository => repository.Get(id))
//                 .Returns(nullResult);

//             // Act
//             var actual = this.chatService.GetChatById(id);

//             // Assert
//             Assert.IsNull(actual);
//         }
//     }
// }

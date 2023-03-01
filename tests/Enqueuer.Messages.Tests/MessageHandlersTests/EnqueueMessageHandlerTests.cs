using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Messages.MessageHandlers;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Telegram.Bot.Types.Chat;
using User = Telegram.Bot.Types.User;

namespace Enqueuer.Messages.Tests.MessageHandlersTests
{
    [TestFixture]
    public class EnqueueMessageHandlerTests
    {
        private Mock<IChatService> chatServiceMock;
        private Mock<IUserService> userServiceMock;
        private Mock<IQueueService> queueServiceMock;
        private Mock<IUserInQueueService> userInQueueServiceMock;
        private EnqueueMessageHandler messageHandler;
        private Mock<ITelegramBotClient> botClientMock;
        public const char Whitespace = ' ';

        [SetUp]
        public void SetUp()
        {
            this.chatServiceMock = new Mock<IChatService>();
            this.userServiceMock = new Mock<IUserService>();
            this.queueServiceMock = new Mock<IQueueService>();
            this.userInQueueServiceMock = new Mock<IUserInQueueService>();
            this.messageHandler = new EnqueueMessageHandler(
                this.chatServiceMock.Object,
                this.userServiceMock.Object,
                this.queueServiceMock.Object,
                this.userInQueueServiceMock.Object);
            this.botClientMock = new Mock<ITelegramBotClient>();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_CommandHasNoQueueName_SendsMessageWithSuggestionToAddQueueName()
        {
            // Arrange
            var message = new Message()
            {
                Text = this.messageHandler.Command,
                Chat = new Chat() { Type = ChatType.Group }
            };

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(EnqueueMessageHandler.PassQueueNameMessage)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [TestCase(-1)]
        [TestCase(0)]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_PositionIsInvalid_DoesNotAddUserToQueue(int invalidPosition)
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1;
            var telegramChat = new Chat() { Type = ChatType.Group };
            var message = new Message()
            {
                Text = string.Join(Whitespace, this.messageHandler.Command, queueName, invalidPosition),
                Chat = telegramChat
            };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(telegramChat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(It.IsAny<Persistence.Models.User>()));

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), invalidPosition))
                .Verifiable();

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default)).Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.userInQueueServiceMock.Verify(userInQueueService => userInQueueService
                .AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_PositionIsInvalid_SendsMessageWithDescription(int invalidPosition)
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1;
            var telegramChat = new Chat() { Type = ChatType.Group };
            var message = new Message()
            {
                Text = string.Join(Whitespace, this.messageHandler.Command, queueName, invalidPosition),
                Chat = telegramChat
            };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(telegramChat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(It.IsAny<Persistence.Models.User>()));

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                It.Is<SendMessageRequest>(
                    request => request.Text.Equals(EnqueueMessageHandler.InvalidQueuePositionMessage)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_QueueDoesNotExist()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1;
            var telegramChat = new Chat() { Id = chatId, Type = ChatType.Group };
            var expectedMessageText = $"There is no queue with name '<b>{queueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.";
            var message = new Message()
            {
                Text = string.Join(Whitespace, this.messageHandler.Command, queueName),
                Chat = telegramChat
            };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(telegramChat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(It.IsAny<Persistence.Models.User>()));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns<Queue>(null);

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_UserAlreadyParticipateInQueue_DoesNotAddUser()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName, Users = new List<UserInQueue>() { new UserInQueue { UserId = userId } } };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService
                    .AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), It.IsAny<int>()))
                .Verifiable();

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.userInQueueServiceMock.Verify(userInQueueService => userInQueueService
                .AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_UserAlreadyParticipateInQueue_SendsMessageWithDescription()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName, Users = new List<UserInQueue>() { new UserInQueue { UserId = userId } } };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var expectedMessageText = $"You're already participating in queue '<b>{queue.Name}</b>'.";
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_SpecifiedPositionIsReserved_DoesNotAddUser()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            const int reservedPosition = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName, Users = new List<UserInQueue>() { new UserInQueue { Position = reservedPosition } } };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName, reservedPosition), Chat = chat };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.IsPositionReserved(queue, reservedPosition))
                .Returns(true);
            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService
                    .AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), It.IsAny<int>()))
                .Verifiable();

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.userInQueueServiceMock.Verify(userInQueueService => userInQueueService
                .AddUserToQueueAsync(It.IsAny<Persistence.Models.User>(), It.IsAny<Queue>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_SpecifiedPositionIsReserved_SendsMessageWithDescription()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            const int reservedPosition = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName, Users = new List<UserInQueue>() { new UserInQueue { Position = reservedPosition } } };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName, reservedPosition), Chat = chat };
            var expectedMessageText = $"Position '<b>{reservedPosition}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.";

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.IsPositionReserved(queue, reservedPosition))
                .Returns(true);
            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_AddsUserToQueueToSpecifiedPosition()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            const int position = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName, position), Chat = chat };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.IsPositionReserved(queue, position))
                .Returns(false);
            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService
                    .AddUserToQueueAsync(user, queue, position))
                .Verifiable();

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.userInQueueServiceMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_PositionIsNotSpecified_AddsUserToQueueToFirstAvailablePosition()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            const int expectedPosition = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.GetFirstAvailablePosition(queue))
                .Returns(expectedPosition);
            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService
                    .AddUserToQueueAsync(user, queue, expectedPosition))
                .Verifiable();

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.userInQueueServiceMock.Verify();
        }

        [Test]
        public async Task EnqueueMessageHandlerTests_HandleMessageAsync_SendsMessageWithDescription()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            const int expectedPosition = 1;
            var user = new Persistence.Models.User() { Id = userId };
            var queue = new Queue() { Name = queueName };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };
            var expectedMessageText = $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{expectedPosition}</b>!";

            this.chatServiceMock.Setup(chatService => chatService.GetOrCreateChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userInQueueServiceMock.Setup(userInQueueService => userInQueueService.GetFirstAvailablePosition(queue))
                .Returns(expectedPosition);
            this.userServiceMock.Setup(userService => userService.GetOrCreateUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }
    }
}

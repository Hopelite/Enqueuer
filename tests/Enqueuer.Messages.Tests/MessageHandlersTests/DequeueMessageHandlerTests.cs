using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Data.Configuration;
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
    public class DequeueMessageHandlerTests
    {
        private Mock<IChatService> chatServiceMock;
        private Mock<IUserService> userServiceMock;
        private Mock<IQueueService> queueServiceMock;
        private DequeueMessageHandler messageHandler;
        private Mock<ITelegramBotClient> botClientMock;
        public const char Whitespace = ' ';

        [SetUp]
        public void SetUp()
        {
            this.chatServiceMock = new Mock<IChatService>();
            this.userServiceMock = new Mock<IUserService>();
            this.queueServiceMock = new Mock<IQueueService>();
            this.messageHandler = new DequeueMessageHandler(
                this.chatServiceMock.Object,
                this.userServiceMock.Object,
                this.queueServiceMock.Object);
            this.botClientMock = new Mock<ITelegramBotClient>();
        }

        [Test]
        public async Task
            DequeueMessageHandlerTests_HandleMessageAsync_CommandHasNoQueueName_SendsMessageWithSuggestionToAddQueueName()
        {
            // Arrange
            var message = new Message() {Text = this.messageHandler.Command, Chat = new Chat() {Type = ChatType.Group}};
            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(DequeueMessageHandler.PassQueueNameMessage)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleMessageAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task DequeueMessageHandlerTests_HandleMessageAsync_QueueDoesNotExist()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            var expectedMessageText = $"There is no queue with name '<b>{queueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.";
            var chat = new Chat() {Id = chatId, Type = ChatType.Group};
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns<Queue>(null);

            this.chatServiceMock.Setup(chatService => chatService.GetNewOrExistingChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetNewOrExistingUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(It.IsAny<Persistence.Models.User>()));

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleMessageAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task DequeueMessageHandlerTests_HandleMessageAsync_UserDoesNotParticipateInQueue()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            var queue = new Queue() {Name = queueName};
            var expectedMessageText = $"You're not participating in queue '<b>{queue.Name}</b>'.";
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);

            this.chatServiceMock.Setup(chatService => chatService.GetNewOrExistingChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetNewOrExistingUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(It.IsAny<Persistence.Models.User>()));

            this.botClientMock.Setup(client => client.MakeRequestAsync(
                    It.Is<SendMessageRequest>(
                        request => request.Text.Equals(expectedMessageText)), default))
                .Verifiable();

            // Act
            await this.messageHandler.HandleMessageAsync(this.botClientMock.Object, message);

            // Assert
            this.botClientMock.Verify();
        }

        [Test]
        public async Task DequeueMessageHandlerTests_HandleMessageAsync_RemovesUserFromQueue()
        {
            // Arrange
            const string queueName = "Test";
            const long chatId = 1L;
            const int userId = 1;
            var user = new Persistence.Models.User() {Id = userId};
            var queue = new Queue() { Name = queueName, Users = new List<UserInQueue>() { new() {UserId = userId} } };
            var chat = new Chat() { Id = chatId, Type = ChatType.Group };
            var message = new Message() { Text = string.Join(Whitespace, this.messageHandler.Command, queueName), Chat = chat };

            this.queueServiceMock.Setup(queueService => queueService.GetChatQueueByName(queueName, chatId))
                .Returns(queue);
            this.queueServiceMock.Setup(queueService => queueService.RemoveUserAsync(queue, user))
                .Verifiable();

            this.chatServiceMock.Setup(chatService => chatService.GetNewOrExistingChatAsync(chat))
                .Returns(Task.FromResult(new Persistence.Models.Chat() { ChatId = chatId }));

            this.userServiceMock.Setup(userService => userService.GetNewOrExistingUserAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(user));

            this.botClientMock.Setup(client => client.MakeRequestAsync(It.IsAny<SendMessageRequest>(), default));

            // Act
            await this.messageHandler.HandleMessageAsync(this.botClientMock.Object, message);

            // Assert
            this.queueServiceMock.Verify();
        }
    }
}

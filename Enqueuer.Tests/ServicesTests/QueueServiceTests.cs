﻿using System.Collections.Generic;
using System.Linq;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Tests.Utilities.Comparers;
using Enqueuer.Tests.Utilities.Wrappers;
using Moq;
using NUnit.Framework;

namespace Enqueuer.Tests.ServicesTests
{
    [TestFixture]
    public class QueueServiceTests
    {
        private readonly Mock<IRepository<Queue>> queueRepositoryMock;
        private readonly QueueService queueService;

        public QueueServiceTests()
        {
            this.queueRepositoryMock = new Mock<IRepository<Queue>>();
            this.queueService = new QueueService(queueRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            this.queueRepositoryMock.Reset();
        }

        [Test]
        public void QueueServiceTests_GetChatQueueByName_ReturnsExistingChatQueue()
        {
            // Arrange
            var comparer = new QueueComparer();
            var expected = new Queue
            {
                Name = "TestQueue",
                ChatId = 1,
            };

            var queues = new List<Queue> { expected }.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueueByName(expected.Name, expected.ChatId);

            // Assert
            Assert.IsTrue(comparer.Equals(expected, actual));
        }

        [Test]
        public void QueueServiceTests_GetChatQueueByName_DifferentChatId_ReturnsNull()
        {
            // Arrange
            const long differentChatId = 2;
            var expected = new Queue
            {
                Name = "TestQueue",
                ChatId = 1,
            };

            var queues = new List<Queue> { expected }.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueueByName(expected.Name, differentChatId);

            // Assert
            Assert.IsNull(actual);
        }

        [Test]
        public void QueueServiceTests_GetChatQueueByName_DifferentQueueName_ReturnsNull()
        {
            // Arrange
            const string differentQueueName = "AnotherTestQueue";
            var expected = new Queue
            {
                Name = "TestQueue",
                ChatId = 1,
            };

            var queues = new List<Queue> { expected }.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueueByName(differentQueueName, expected.ChatId);

            // Assert
            Assert.IsNull(actual);
        }

        [Test]
        public void QueueServiceTests_GetChatQueueByName_BotheQueueNameAndChatIdDifferent_ReturnsNull()
        {
            // Arrange
            const string differentQueueName = "AnotherTestQueue";
            const long differentChatId = 2;
            var expected = new Queue
            {
                Name = "TestQueue",
                ChatId = 1,
            };

            var queues = new List<Queue> { expected }.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueueByName(differentQueueName, differentChatId);

            // Assert
            Assert.IsNull(actual);
        }

        [Test]
        public void QueueServiceTests_GetChatQueues_ReturnsAllQueues()
        {
            // Arrange
            const int chatId = 1;
            var comparer = new QueueComparer();
            var expected = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", ChatId = chatId },
                new Queue() { Name = "AnotherQueue", ChatId = chatId },
                new Queue() { Name = "TwiceAsAnotherQueue", ChatId = chatId },
            };

            var queues = expected.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            AssertWrapper.StrictMultipleEquals(expected, actual);
        }

        [Test]
        public void QueueServiceTests_GetChatQueues_ReturnsExistingChatQueues()
        {
            // Arrange
            const int chatId = 1;
            var comparer = new QueueComparer();
            var queues = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", ChatId = chatId },
                new Queue() { Name = "AnotherQueue", ChatId = chatId },
                new Queue() { Name = "TwiceAsAnotherQueue", ChatId = 2 },
            };

            var expected = queues.Where(queue => queue.ChatId == chatId);
            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues.AsQueryable());

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            AssertWrapper.MultipleEquals(expected, actual);
        }

        [Test]
        public void QueueServiceTests_GetChatQueues_ReturnsNoQueues()
        {
            // Arrange
            const int chatId = 1;
            var comparer = new QueueComparer();
            var queues = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", ChatId = 2 },
                new Queue() { Name = "AnotherQueue", ChatId = 2 },
                new Queue() { Name = "TwiceAsAnotherQueue", ChatId = 2 },
            };

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues.AsQueryable());

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            Assert.IsEmpty(actual);
        }

        [Test]
        public void QueueServiceTests_GetTelegramChatQueues_ReturnsAllQueues()
        {
            // Arrange
            const long chatId = 1;
            var comparer = new QueueComparer();
            var chat = new Chat() { ChatId = chatId };

            var expected = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", Chat = chat },
                new Queue() { Name = "AnotherQueue", Chat = chat },
                new Queue() { Name = "TwiceAsAnotherQueue", Chat = chat },
            };

            var queues = expected.AsQueryable();

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues);

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            AssertWrapper.StrictMultipleEquals(expected, actual);
        }

        [Test]
        public void QueueServiceTests_GetTelegramChatQueues_ReturnsExistingChatQueues()
        {
            // Arrange
            const long chatId = 1;
            var comparer = new QueueComparer();
            var chat = new Chat() { ChatId = chatId };
            var anotherChat = new Chat() { ChatId = 2 };

            var queues = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", Chat = chat },
                new Queue() { Name = "AnotherQueue", Chat = chat },
                new Queue() { Name = "TwiceAsAnotherQueue", Chat = anotherChat },
            };

            var expected = queues.Where(queue => queue.Chat.ChatId == chatId);
            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues.AsQueryable());

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            AssertWrapper.MultipleEquals(expected, actual);
        }

        [Test]
        public void QueueServiceTests_GetTelegramChatQueues_ReturnsNoQueues()
        {
            // Arrange
            const int chatId = 1;
            var comparer = new QueueComparer();
            var chat = new Chat() { ChatId = 2 };
            var queues = new List<Queue>()
            {
                new Queue() { Name = "TestQueue", Chat = chat },
                new Queue() { Name = "AnotherQueue", Chat = chat },
                new Queue() { Name = "TwiceAsAnotherQueue", Chat = chat },
            };

            this.queueRepositoryMock.Setup(repository => repository.GetAll())
                .Returns(queues.AsQueryable());

            // Act
            var actual = this.queueService.GetChatQueues(chatId);

            // Assert
            Assert.IsEmpty(actual);
        }
    }
}

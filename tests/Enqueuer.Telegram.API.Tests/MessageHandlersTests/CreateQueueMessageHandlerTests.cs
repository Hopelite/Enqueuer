using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Data.Constants;
using Enqueuer.Persistence;
using Enqueuer.Telegram.API.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Enqueuer.Telegram.API.Tests.MessageHandlersTests;

public class CreateQueueMessageHandlerTests
{
    private readonly EnqueuerApplication _application;

    public CreateQueueMessageHandlerTests()
    {
        _application = new EnqueuerApplication();
    }

    [Fact]
    public async Task QueueNameProvided_CreatesQueue()
    {
        const string groupTitle = "Test";
        const string userFirstName = "TestUser";
        const string queueName = "TestQueue";

        var update = new Update
        {
            Id = 1,
            Message = new Message
            {
                MessageId = 1,
                Date = DateTime.Now,
                From = new User
                {
                    Id = 1,
                    IsBot = false,
                    FirstName = userFirstName,
                },
                Chat = new Chat
                {
                    Id = 1,
                    Type = ChatType.Group,
                    Title = groupTitle,
                },
                Text = $"{MessageConstants.CreateQueueCommand} {queueName}",
            }
        };

        var client = _application.CreateApplicationClient();

        await client.PostAsync(update);

        using var scope = _application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queue = dbContext.Queues.Include(q => q.Group).FirstOrDefault(q => q.Name.Equals(queueName));
        Assert.NotNull(queue);
        Assert.Equal(groupTitle, queue.Group.Title);
    }
}

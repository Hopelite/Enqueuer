using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Telegram.API.Tests.Utilities;
using Enqueuer.Telegram.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Enqueuer.Telegram.API.Tests;

public sealed class EnqueuerApiTests : IDisposable
{
    private readonly EnqueuerApplication _application;

    public EnqueuerApiTests()
    {
        _application = new EnqueuerApplication();
    }

    public void Dispose()
    {
        _application.Dispose();
    }

    [Theory]
    [InlineData(MessageCommands.CreateQueueCommand)]
    [InlineData(MessageCommands.DequeueCommand)]
    [InlineData(MessageCommands.EnqueueCommand)]
    [InlineData(MessageCommands.QueueCommand)]
    [InlineData(MessageCommands.RemoveQueueCommand)]
    public async Task PostMessage_NewUserAndGroup_SavesUserAndGroup(string command)
    {
        const string groupTitle = "Test";
        const string userFirstName = "TestUser";

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
                Text = command,
            }
        };

        var client = _application.CreateApplicationClient();

        await client.PostAsync(update);

        using var scope = _application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = dbContext.Users.FirstOrDefault(u => u.FirstName.Equals(userFirstName));
        Assert.NotNull(user);

        var group = dbContext.Groups.Include(g => g.Members).FirstOrDefault(g => g.Title.Equals(groupTitle));
        Assert.NotNull(group);

        Assert.Contains(group.Members, m => m.Id == user.Id);
    }
}

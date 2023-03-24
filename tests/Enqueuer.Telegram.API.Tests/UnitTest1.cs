using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Enqueuer.Telegram.API.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var service = new EnqueuerApplicationFactory().CreateClient();

        var content = new StringContent(JsonConvert.SerializeObject(new Update
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
                    FirstName = "Test",
                },
                Chat = new Chat
                {
                    Id = 1,
                    Type = ChatType.Group,
                    Title = "Test",
                },
                Text = "/createqueue Test"
            }
        }), Encoding.UTF8, "application/json");

        var response = await service.PostAsync("http://localhost:8443/bot{ACCESS_TOKEN}", content);
    }
}

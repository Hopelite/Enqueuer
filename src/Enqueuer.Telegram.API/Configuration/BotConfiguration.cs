using System;
using Enqueuer.Data.Configuration;
using Microsoft.Extensions.Configuration;

namespace Enqueuer.Telegram.Configuration;

public class BotConfiguration : IBotConfiguration
{
    private const string BotConfigurationSectionName = "BotConfiguration";

    public string AccessToken { get; }

    public int QueuesPerChat { get; }

    public string BotVersion { get; }

    public long DevelopmentChatId { get; }

    public BotConfiguration(IConfiguration configuration)
    {
        var botSection = configuration.GetSection(BotConfigurationSectionName);
        AccessToken = botSection.GetRequiredSection("AccessToken").Value;
        QueuesPerChat = int.Parse(botSection.GetRequiredSection("QueuesPerChat").Value);
        BotVersion = botSection.GetRequiredSection("BotVersion").Value;
        DevelopmentChatId = long.Parse(botSection.GetRequiredSection("DevelopmentChatId").Value);
    }
}

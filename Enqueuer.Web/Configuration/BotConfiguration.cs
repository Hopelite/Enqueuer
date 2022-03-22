using Enqueuer.Data.Configuration;
using Microsoft.Extensions.Configuration;

namespace Enqueuer.Web.Configuration
{
    /// <inheritdoc/>
    public class BotConfiguration : IBotConfiguration
    {
        private const string BotConfigurationSectionName = "BotConfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotConfiguration"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> with application configuration.</param>
        public BotConfiguration(IConfiguration configuration)
        {
            var botSection = configuration.GetSection(BotConfigurationSectionName);
            this.AccessToken = botSection.GetRequiredSection("AccessToken").Value;
            this.QueuesPerChat = int.Parse(botSection.GetRequiredSection("QueuesPerChat").Value);
            this.BotVersion = botSection.GetRequiredSection("BotVersion").Value;
            this.DevelopmentChatId = long.Parse(botSection.GetRequiredSection("DevelopmentChatId").Value);
        }

        /// <inheritdoc/>
        public string AccessToken { get; set; }

        /// <inheritdoc/>
        public string ApplicationHost { get; set; }

        /// <inheritdoc/>
        public int QueuesPerChat { get; set; }

        /// <inheritdoc/>
        public string BotVersion { get; set; }

        /// <inheritdoc/>
        public long DevelopmentChatId { get; set; }
    }
}

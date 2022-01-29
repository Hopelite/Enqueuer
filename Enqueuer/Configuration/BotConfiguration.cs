using Microsoft.Extensions.Configuration;

namespace Enqueuer.Web.Configuration
{
    /// <inheritdoc/>
    public class BotConfiguration : IBotConfiguration
    {
        private const string botConfigurationSectionName = "BotConfiguration";

        /// <inheritdoc/>
        public BotConfiguration(IConfiguration configuration)
        {
            var botSection = configuration.GetSection(botConfigurationSectionName);
            this.AccessToken = botSection.GetRequiredSection("AccessToken").Value;
            this.ApplicationHost = botSection.GetRequiredSection("ApplicationHost").Value;
        }

        /// <inheritdoc/>
        public string AccessToken { get; set; }

        /// <inheritdoc/>
        public string ApplicationHost { get; set; }
    }
}

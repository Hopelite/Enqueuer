using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Utilities.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Web.Configuration
{
    /// <summary>
    /// Sets webhook when application starts and removes if right upon shutdown.
    /// </summary>
    public class ConfigureWebhook : IHostedService
    {
        private readonly ILogger<ConfigureWebhook> logger;
        private readonly IServiceProvider services;
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureWebhook"/> class.
        /// </summary>
        /// <param name="logger">Logger to log info.</param>
        /// <param name="serviceProvider">Application <see cref="IServiceProvider"/>.</param>
        /// <param name="botConfiguration"><see cref="IBotConfiguration"/> with bot configuration.</param>
        public ConfigureWebhook(
            ILogger<ConfigureWebhook> logger,
            IServiceProvider serviceProvider,
            IBotConfiguration botConfiguration)
        {
            this.logger = logger;
            this.services = serviceProvider;
            this.botConfiguration = botConfiguration;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = this.services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            var webhookAddress = @$"{botConfiguration.ApplicationHost}/bot{botConfiguration.AccessToken}";
            this.logger.LogInformation("Setting webhook: {webhookAddress}", webhookAddress);
            await botClient.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = this.services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            this.logger.LogInformation("Removing webhook");
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}

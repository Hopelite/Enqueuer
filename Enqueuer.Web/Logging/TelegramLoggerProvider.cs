using Enqueuer.Data.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Enqueuer.Web.Logging
{
    public class TelegramLoggerProvider : ILoggerProvider
    {
        private readonly ITelegramBotClient telegramBotClient;
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelegramLoggerProvider"/> class.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to send messages by.</param>
        /// <param name="botConfiguration"><see cref="IBotConfiguration"/> to rely on.</param>
        public TelegramLoggerProvider(ITelegramBotClient telegramBotClient, IBotConfiguration botConfiguration)
        {
            this.telegramBotClient = telegramBotClient;
            this.botConfiguration = botConfiguration;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new TelegramLogger(this.telegramBotClient, this.botConfiguration.DevelomentChatId);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}

using Enqueuer.Data.Configuration;
using Enqueuer.Web.Logging;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Enqueuer.Web.Extensions
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddTelegramLogger(this ILoggerFactory loggerFactory, ITelegramBotClient telegramBotClient, IBotConfiguration botConfiguration)
        {
            loggerFactory.AddProvider(new TelegramLoggerProvider(telegramBotClient, botConfiguration));
            return loggerFactory;
        }
    }
}

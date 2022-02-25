using Microsoft.Extensions.Logging;
using System;
using Telegram.Bot;

namespace Enqueuer.Web.Logging
{
    /// <summary>
    /// Sends logs as messages to the specified Telegram chat.
    /// </summary>
    public class TelegramLogger : ILogger
    {
        private readonly LogLevel minimalLogLevel;
        private readonly ITelegramBotClient telegramBotClient;
        private readonly long chatId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelegramLoggerProvider"/> class.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to send messages by.</param>
        /// <param name="chatId">Telegram chat ID to send messages to.</param>
        /// <param name="minimalLogLevel">Minimal <see cref="LogLevel"/> to log errors that are equal or greater. Set to Information level by default.</param>
        public TelegramLogger(ITelegramBotClient telegramBotClient, long chatId, LogLevel minimalLogLevel = LogLevel.Information)
        {
            this.telegramBotClient = telegramBotClient;
            this.chatId = chatId;
            this.minimalLogLevel = minimalLogLevel;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel <= this.minimalLogLevel;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.IsEnabled(logLevel))
            {
                var message = formatter?.Invoke(state, exception) ?? BuildDefaultMessage(state, exception);
                this.telegramBotClient.SendTextMessageAsync(this.chatId, message).Wait();
            }
        }

        private static string BuildDefaultMessage<TState>(TState state, Exception exception)
        {
            return "An exception thrown at " + state.ToString() + Environment.NewLine +
                   "Exception message: " + exception.Message + Environment.NewLine +
                   "Stack trace:" + exception.StackTrace ?? "empty";
        }
    }
}

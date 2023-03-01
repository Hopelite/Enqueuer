namespace Enqueuer.Data.Configuration
{
    /// <summary>
    /// Contains bot configuration.
    /// </summary>
    public interface IBotConfiguration
    {
        /// <summary>
        /// Gets the bot access token.
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Gets the maximal number of queues per chat.
        /// </summary>
        public int QueuesPerChat { get; }

        /// <summary>
        /// Gets the bot version.
        /// </summary>
        public string BotVersion { get; }

        /// <summary>
        /// Gets the bot development chat ID to send errors to.
        /// </summary>
        public long DevelopmentChatId { get; }
    }
}

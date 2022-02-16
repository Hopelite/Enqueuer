namespace Enqueuer.Utilities.Configuration
{
    /// <summary>
    /// Contains bot configuration.
    /// </summary>
    public interface IBotConfiguration
    {
        /// <summary>
        /// Gets or sets bot access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets application host.
        /// </summary>
        public string ApplicationHost { get; set;  }

        /// <summary>
        /// Gets or sets maximal number of queues per chat.
        /// </summary>
        public int QueuesPerChat { get; set; }

        /// <summary>
        /// Gets or sets bot version.
        /// </summary>
        public string BotVersion { get; set; }
    }
}

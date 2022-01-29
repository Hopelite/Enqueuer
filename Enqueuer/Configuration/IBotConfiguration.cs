namespace Enqueuer.Web.Configuration
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
    }
}

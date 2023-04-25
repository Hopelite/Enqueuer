using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.Messages.Groups
{
    /// <summary>
    /// Contains requested groups in which the Telegram user participates.
    /// </summary>
    public class GetUserGroupsResponse
    {
        /// <summary>
        /// Groups in which the Telegram user participates.
        /// </summary>
        public Group[] Groups { get; set; }
    }
}

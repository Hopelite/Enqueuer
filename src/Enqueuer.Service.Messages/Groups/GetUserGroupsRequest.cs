namespace Enqueuer.Service.Messages.Groups
{
    /// <summary>
    /// Request known to the bot groups in which the Telegram user participates.
    /// </summary>
    public class GetUserGroupsRequest
    {
        /// <summary>
        /// Telegram user ID whose groups to request.
        /// </summary>
        public long UserId { get; set; }
    }
}

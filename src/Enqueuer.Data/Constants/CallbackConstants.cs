namespace Enqueuer.Data.Constants
{
    /// <summary>
    /// Contains callback constants.
    /// </summary>
    public static class CallbackConstants
    {
        /// <summary>
        /// Gets the list chats command.
        /// </summary>
        public static string ListChatsCommand => "/l";

        /// <summary>
        /// Gets the get chat command.
        /// </summary>
        public static string GetChatCommand => "/gc";

        /// <summary>
        /// Gets the get queue command.
        /// </summary>
        public static string GetQueueCommand => "/gq";

        /// <summary>
        /// Gets the dequeue me command.
        /// </summary>
        public static string DequeueMeCommand => "/dq";

        /// <summary>
        /// Gets the enqueue command.
        /// </summary>
        public static string EnqueueCommand => "/eq";

        /// <summary>
        /// Gets the remove queue command.
        /// </summary>
        public static string RemoveQueueCommand => "/rq";

        /// <summary>
        /// Gets the enqueue me command.
        /// </summary>
        public static string EnqueueMeCommand => "/eqm";

        /// <summary>
        /// Gets the enqueue at command.
        /// </summary>
        public static string EnqueueAtCommand => "/eqa";

        /// <summary>
        /// Gets the switch queue dynamic status command.
        /// </summary>
        public static string SwitchQueueDynamicCommand => "/sqd";
    }
}

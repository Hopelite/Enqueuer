namespace Enqueuer.Messaging.Core.Constants;

/// <summary>
/// Contains callback commands.
/// </summary>
public static class CallbackCommands
{
    /// <summary>
    /// Gets the list chats command.
    /// </summary>
    public const string ListChatsCommand = "l";

    /// <summary>
    /// Gets the get chat command.
    /// </summary>
    public const string GetChatCommand = "gc";

    /// <summary>
    /// Gets the get queue command.
    /// </summary>
    public const string GetQueueCommand = "gq";

    /// <summary>
    /// Gets the dequeue me command.
    /// </summary>
    public const string DequeueMeCommand = "dq";

    /// <summary>
    /// Gets the enqueue command.
    /// </summary>
    public const string EnqueueCommand = "eq";

    /// <summary>
    /// Gets the remove queue command.
    /// </summary>
    public const string RemoveQueueCommand = "rq";

    /// <summary>
    /// Gets the enqueue me command.
    /// </summary>
    public const string EnqueueMeCommand = "eqm";

    /// <summary>
    /// Gets the enqueue at command.
    /// </summary>
    public const string EnqueueAtCommand = "eqa";

    /// <summary>
    /// Gets the switch queue dynamic status command.
    /// </summary>
    public const string SwitchQueueDynamicCommand = "sqd";

    /// <summary>
    /// Exchange positions command.
    /// </summary>
    public const string ExchangePositionsCommand = "ep";
}

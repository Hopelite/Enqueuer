using Enqueuer.Service.Messages.Validation;

namespace Enqueuer.Service.Messages;

public class CreateQueueRequest
{
    /// <summary>
    /// ID of the group to which this queue belongs.
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// Queue name.
    /// </summary>
    [QueueName]
    public string QueueName { get; set; } = null!;

    /// <summary>
    /// Queue creator's ID.
    /// </summary>
    public long CreatorId { get; set; }
}

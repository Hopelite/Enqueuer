using Enqueuer.Service.Messages.Validation;

namespace Enqueuer.Service.Messages.Models;

public class Queue
{
    public int Id { get; set; }

    [QueueName]
    public string Name { get; set; } = null!;
}

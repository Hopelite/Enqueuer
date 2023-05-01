namespace Enqueuer.Service.Messages.Models;

public class QueueInfo : Queue
{
    public User? Creator { get; set; }

    public QueueMember[] Members { get; set; } = null!;
}

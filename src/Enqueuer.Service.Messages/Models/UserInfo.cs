namespace Enqueuer.Service.Messages.Models;

public class UserInfo : User
{
    public Group[] Groups { get; set; } = null!;
}

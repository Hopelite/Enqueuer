using System.Text.Json.Serialization;

namespace Enqueuer.Messaging.Core.Types.Common;

public class User : Group
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    [JsonIgnore]
    public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";

    // TODO: remove
    public static implicit operator Telegram.Bot.Types.User(User user) => new Telegram.Bot.Types.User()
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Id = user.Id
    };
}

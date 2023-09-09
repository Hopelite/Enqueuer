using System.Text.Json.Serialization;

namespace Enqueuer.Messaging.Core.Types.Common;

/// <summary>
/// Represents a Telegram chat with a user.
/// </summary>
public class User : Chat
{
    /// <summary>
    /// The first name of the user.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Optional. The last name of the user.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    [JsonIgnore]
    public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";

    // TODO: remove
    public static implicit operator Telegram.Bot.Types.User(User user) => new()
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Id = user.Id
    };
}

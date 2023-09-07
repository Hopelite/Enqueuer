using System.Text.Json.Serialization;

namespace Enqueuer.Messaging.Core.Types.Common;

public class User : Group
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    [JsonIgnore]
    public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";
}

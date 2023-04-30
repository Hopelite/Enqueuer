using System.ComponentModel.DataAnnotations;

namespace Enqueuer.Service.Messages.Models;

/// <summary>
/// Represents a Telegram user.
/// </summary>
public class User
{
    /// <summary>
    /// Telegram group ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    [Required(ErrorMessage = "The first name property is required.")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "The length of the first name cannot exceed 64 characters.")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    ///  User's last name.
    /// </summary>
    [StringLength(64, MinimumLength = 1, ErrorMessage = "The length of the first name cannot exceed 64 characters.")]
    public string? LastName { get; set; }
}

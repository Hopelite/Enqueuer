using System.ComponentModel.DataAnnotations;

namespace Enqueuer.Service.Messages.Models;

/// <summary>
/// Represents a Telegram group or supergroup.
/// </summary>
public class Group
{
    /// <summary>
    /// Telegram group ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Telegram group title.
    /// </summary>
    [Required(ErrorMessage = "The group title property is required.")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "The length of the group title cannot exceed 128 characters.")]
    public string Title { get; set; } = null!;
}


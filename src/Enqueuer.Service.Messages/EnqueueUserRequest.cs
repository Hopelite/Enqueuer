using System.ComponentModel.DataAnnotations;
using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.Messages;

public class EnqueueUserRequest
{
    public int? Position { get; set; }

    [Required]
    public User User { get; set; } = null!;
}

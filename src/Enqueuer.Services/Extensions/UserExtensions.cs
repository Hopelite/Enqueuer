using System;
using System.Collections.Generic;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Extensions;

internal static class UserExtensions
{
    public static User ConvertToEntity(this Telegram.Bot.Types.User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Groups = new List<Group>(),
            ParticipatesIn = new List<QueueMember>(),
            CreatedQueues = new List<Queue>(),
        };
    }
}

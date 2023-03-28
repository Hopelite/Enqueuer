using System;
using System.Collections.Generic;
using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Extensions;

public static class GroupExtensions
{
    public static Group ConvertToGroup(this Telegram.Bot.Types.Chat telegramChat)
    {
        return new Group
        {
            Id = telegramChat.Id,
            Title = telegramChat.Title!,
            Members = new List<User>(),
            Queues = new List<Queue>(),
        };
    }

    public static Queue? GetQueueByName(this Group group, string queueName)
    {
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        return group.Queues?.FirstOrDefault(q => q.Name.Equals(queueName));
    }

    public static bool HasQueue(this Group group, string queueName)
    {
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentNullException(nameof(queueName));
        }

        if (group.Queues == null)
        {
            return false;
        }

        return group.Queues.Any(q => q.Name.Equals(queueName));
    }
}

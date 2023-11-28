﻿using Enqueuer.Messaging.Core.Types.Common;
using Enqueuer.Messaging.Core.Types.Messages;

namespace Enqueuer.Telegram.Messages.Extensions;

public static class MessageQueryExtensions
{
    private const char Whitespace = ' ';

    /// <summary>
    /// Gets queue name from <paramref name="commandContext"/>.
    /// </summary>
    public static string GetQueueName(this CommandContext commandContext)
    {
        return commandContext.Parameters.GetQueueName(startIndex: 0);
    }

    /// <summary>
    /// Gets queue name from <paramref name="commandParameters"/>.
    /// </summary>
    public static string GetQueueName(this string[] commandParameters)
    {
        return commandParameters.GetQueueName(startIndex: 0);
    }

    /// <summary>
    /// Gets queue name from <paramref name="query"/>.
    /// </summary>
    /// <param name="startIndex">Start index where queue name starts.</param>
    public static string GetQueueName(this string[] query, int startIndex = 1) // TODO: add unit tests
    {
        if (query.Length == 2 && int.TryParse(query[^1], out var _))
        {
            return query[^1];
        }

        if (int.TryParse(query[^1], out var _))
        {
            return string.Join(separator: Whitespace, query[startIndex..^1]);
        }

        return string.Join(separator: Whitespace, query[startIndex..]);
    }

    /// <summary>
    /// Gets queue name from <paramref name="query"/> without user number.
    /// </summary>
    /// <param name="startIndex">Start index where queue name starts.</param>
    public static string GetQueueNameWithoutUserPosition(this string[] query)
    {
        return string.Join(separator: Whitespace, query[..^1]);
    }

    /// <summary>
    /// Checks if <paramref name="query"/> has parameters provided.
    /// </summary>
    public static bool HasParameters(this string[]? query)
    {
        return query?.Length > 1;
    }

    /// <summary>
    /// Checks if <paramref name="messageContext"/> has parameters provided.
    /// </summary>
    public static bool HasParameters(this MessageContext messageContext)
    {
        if (messageContext.Command == null)
        {
            return false;
        }

        return messageContext.Command.Parameters.Length > 0;
    }
}

namespace Enqueuer.Telegram.Messages.Extensions;

public static class MessageQueryExtensions
{
    private const char Whitespace = ' ';

    /// <summary>
    /// Gets queue name from <paramref name="query"/>.
    /// </summary>
    /// <param name="startIndex">Start index where queue name starts.</param>
    public static string GetQueueName(this string[] query, int startIndex = 1)
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
    public static string GetQueueNameWithoutUserPosition(this string[] query, int startIndex = 1)
    {
        return string.Join(separator: Whitespace, query[startIndex..^1]);
    }

    /// <summary>
    /// Checks if <paramref name="query"/> has parameters provided.
    /// </summary>
    public static bool HasParameters(this string[] query)
    {
        return query.Length > 1;
    }
}

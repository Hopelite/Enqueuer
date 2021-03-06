namespace Enqueuer.Messages.Extensions
{
    /// <summary>
    /// Contains extension methods for array of strings.
    /// </summary>
    public static class StringArrayExtensions
    {
        private const char Whitespace = ' ';

        /// <summary>
        /// Gets queue name from <paramref name="query"/>.
        /// </summary>
        /// <param name="query">Query to extract queue name from.</param>
        /// <param name="startIndex">Start index where queue name starts.</param>
        /// <returns>Queue name specified in <paramref name="query"/>.</returns>
        public static string GetQueueName(this string[] query, int startIndex = 1)
        {
            return string.Join(separator: Whitespace, query[startIndex..]);
        }

        /// <summary>
        /// Gets queue name from <paramref name="query"/> without user number.
        /// </summary>
        /// <param name="query">Query to extract queue name from.</param>
        /// <param name="startIndex">Start index where queue name starts.</param>
        /// <returns>Queue name specified in <paramref name="query"/>.</returns>
        public static string GetQueueNameWithoutUserPosition(this string[] query, int startIndex = 1)
        {
            return string.Join(separator: Whitespace, query[startIndex..^1]);
        }

        /// <summary>
        /// Checks if <paramref name="query"/> has parameters provided.
        /// </summary>
        /// <param name="query">Query to check.</param>
        /// <returns>True, if <paramref name="query"/> has parameters; false otherwise.</returns>
        public static bool HasParameters(this string[] query)
        {
            return query.Length > 1;
        }
    }
}

using System;

namespace Enqueuer.Messages.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        private const char Whitespace = ' ';

        /// <summary>
        /// Splits <paramref name="messageText"/> to words by removing whitespaces.
        /// </summary>
        /// <param name="messageText">String to split to words.</param>
        /// <returns>Array of message words.</returns>
        public static string[] SplitToWords(this string messageText)
        {
            return messageText.Split(separator: Whitespace, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

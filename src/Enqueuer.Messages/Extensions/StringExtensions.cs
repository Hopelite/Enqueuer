using System;

namespace Enqueuer.Messages.Extensions;

public static class StringExtensions
{
    private const char Whitespace = ' ';

    /// <summary>
    /// Splits <paramref name="messageText"/> to words by removing whitespaces.
    /// </summary>
    /// <returns>Array of message words.</returns>
    public static string[] SplitToWords(this string messageText)
    {
        return messageText.Split(separator: Whitespace, StringSplitOptions.RemoveEmptyEntries);
    }
}

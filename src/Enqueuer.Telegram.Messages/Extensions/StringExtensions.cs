using System;

namespace Enqueuer.Telegram.Messages.Extensions;

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

    public static bool TryGetCommand(this string messageText, out string command)
    {
        command = messageText.SplitToWords()[0];
        if (command[0] != '/')
        {
            return false;
        }

        var botNamePosition = messageText.IndexOf('@');
        if (botNamePosition > 0)
        {
            command = command[..botNamePosition];
        }

        return true;
    }
}

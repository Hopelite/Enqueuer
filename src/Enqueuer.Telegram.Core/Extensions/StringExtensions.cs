using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Core.Types.Common;

namespace Enqueuer.Telegram.Core.Extensions;

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
    
    /// <summary>
    /// Tries to create <paramref name="commandContext"/> from <paramref name="messageText"/>.
    /// </summary>
    public static bool TryGetCommand(this string messageText, [NotNullWhen(returnValue: true)] out CommandContext? commandContext)
    {
        commandContext = null;
        if (TryGetCommand(messageText, out var command, out var parameters))
        {
            commandContext = new CommandContext(command, parameters);
            return true;
        }

        return false;
    }

    private static bool TryGetCommand(this string messageText, [NotNullWhen(returnValue: true)] out string? command, [NotNullWhen(returnValue: true)] out string[]? parameters)
    {
        command = null;
        parameters = null;

        var commandWords = messageText.SplitToWords();
        if (commandWords.Length == 0)
        {
            return false;
        }

        command = commandWords[0];
        if (command[0] != '/')
        {
            return false;
        }

        var botNamePosition = messageText.IndexOf('@');
        if (botNamePosition > 0)
        {
            command = command[..botNamePosition];
        }

        parameters = commandWords[1..];
        return true;
    }
}

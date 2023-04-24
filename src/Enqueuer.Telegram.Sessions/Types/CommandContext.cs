using System;
using System.Collections.Generic;

namespace Enqueuer.Telegram.Sessions.Types;

/// <summary>
/// Contains necessary data related to the command.
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Command text without parameters.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Command parameters that were either specified after the command text or added to the context later. 
    /// </summary>
    public IDictionary<string, CommandParameter> Parameters { get; } = new Dictionary<string, CommandParameter>();

    public CommandContext(string command)
    {
        Command = string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentNullException(nameof(command), "Command can't be null, empty or a whitespace.")
            : command;
    }

    internal CommandContext(string command, IDictionary<string, CommandParameter> parameters)
    {
        Command = string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentNullException(nameof(command), "Command can't be null, empty or a whitespace.")
            : command;
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}

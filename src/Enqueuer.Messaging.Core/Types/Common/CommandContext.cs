using System;
using System.Text.Json.Serialization;

namespace Enqueuer.Messaging.Core.Types.Common;

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
    public string[] Parameters { get; }

    public CommandContext(string command)
        : this(command, Array.Empty<string>())
    {
    }

    [JsonConstructor]
    public CommandContext(string command, string[] parameters)
    {
        Command = string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentNullException(nameof(command), "Command can't be null, empty or a whitespace.")
            : command;

        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}

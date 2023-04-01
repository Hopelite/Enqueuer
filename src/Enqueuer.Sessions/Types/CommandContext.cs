using System;

namespace Enqueuer.Sessions.Types;

/// <summary>
/// Contains necessary data related to the command.
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Command text without parameters.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Parameters that were specified after the command. 
    /// </summary>
    public CommandParameter[] Parameters { get; set; } = Array.Empty<CommandParameter>();
}

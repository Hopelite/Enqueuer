using System;
using Enqueuer.Telegram.Sessions.Exceptions;

namespace Enqueuer.Telegram.Sessions.Types;

#pragma warning disable CS8618

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter
{
    private readonly string _name;
    private readonly string? _textValue;
    private readonly long? _longValue;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name 
    {
        get => _name;
        init => _name = string.IsNullOrWhiteSpace(value) 
            ? throw new ArgumentNullException(nameof(value), "Parameter name can't be null, empty or a whitespace.")
            : value;
    }

    /// <summary>
    /// Value of the text type parameter.
    /// </summary>
    /// <exception cref="ParameterHasDifferentTypeException">
    /// Thrown, if the parameter has a different type.
    /// </exception>
    public string TextValue
    {
        get => _textValue ?? throw new ParameterHasDifferentTypeException($"Parameter \"{Name}\" does not have text value.");
        init => _textValue = value;
    }

    /// <summary>
    /// Value of the <see cref="long"/> type parameter.
    /// </summary>
    /// <exception cref="ParameterHasDifferentTypeException">
    /// Thrown, if the parameter has a different type.
    /// </exception>
    public long LongValue
    {
        get => _longValue ?? throw new ParameterHasDifferentTypeException($"Parameter \"{Name}\" does not have long value.");
        init => _longValue = value;
    }
    
    public CommandParameter(string name, string textValue)
    {
        Name = name;
        TextValue = textValue;
    }

    public CommandParameter(string name, long longValue)
    {
        Name = name;
        LongValue = longValue;
    }
}

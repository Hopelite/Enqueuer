using System;
using System.Text.Json.Serialization;
using Enqueuer.Messaging.Core.Exceptions;

namespace Enqueuer.Messaging.Core.Types.Common;

#pragma warning disable CS8618

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter // TODO: complete this idea
{
    private readonly string _name;
    private readonly string? _textValue;
    private readonly long? _longValue;
    private readonly int? _intValue;

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

    /// <summary>
    /// Value of the <see cref="int"/> type parameter.
    /// </summary>
    /// <exception cref="ParameterHasDifferentTypeException">
    /// Thrown, if the parameter has a different type.
    /// </exception>
    public int IntValue
    {
        get => _intValue ?? throw new ParameterHasDifferentTypeException($"Parameter \"{Name}\" does not have int value.");
        init => _intValue = value;
    }

    [JsonConstructor]
    public CommandParameter()
    {
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

    public CommandParameter(string name, int intValue)
    {
        Name = name;
        IntValue = intValue;
    }
}

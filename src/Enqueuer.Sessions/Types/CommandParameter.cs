using System;
using Enqueuer.Sessions.Exceptions;

namespace Enqueuer.Sessions.Types;

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter
{
    private readonly string? _textValue;
    private readonly long? _longValue;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Value of the text type parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown, if the value is null, which means that the parameter has a different type.
    /// </exception>
    public string TextValue
    {
        get => _textValue ?? throw new ParameterHasDifferentTypeException($"Parameter \"{Name}\" does not have text value.");
        init => _textValue = value;
    }

    /// <summary>
    /// Value of the <see cref="long"/> type parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown, if the value is null, which means that the parameter has a different type.
    /// </exception>
    public long LongValue
    {
        get => _longValue ?? throw new ParameterHasDifferentTypeException($"Parameter \"{Name}\" does not have long value.");
        init => _longValue = value;
    }
}

using System;
using System.Runtime.Serialization;

namespace Enqueuer.Messaging.Core.Exceptions;
/// <summary>
/// Thrown when the parameter has a type other than expected.
/// </summary>
[Serializable]
public class ParameterHasDifferentTypeException : Exception
{
    public ParameterHasDifferentTypeException()
    {
    }

    public ParameterHasDifferentTypeException(string? message)
        : base(message)
    {
    }

    public ParameterHasDifferentTypeException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ParameterHasDifferentTypeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

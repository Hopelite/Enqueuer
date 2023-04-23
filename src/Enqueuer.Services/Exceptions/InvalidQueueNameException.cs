using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class InvalidQueueNameException : Exception
{
    public InvalidQueueNameException()
    {
    }

    public InvalidQueueNameException(string? message)
        : base(message)
    {
    }

    public InvalidQueueNameException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected InvalidQueueNameException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

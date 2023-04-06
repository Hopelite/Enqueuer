using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class QueueDoesNotExistException : Exception
{
    public QueueDoesNotExistException()
    {
    }

    public QueueDoesNotExistException(string? message)
        : base(message)
    {
    }

    public QueueDoesNotExistException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected QueueDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

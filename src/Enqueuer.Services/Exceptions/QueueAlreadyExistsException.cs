using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class QueueAlreadyExistsException : Exception
{
    public QueueAlreadyExistsException()
    {
    }

    public QueueAlreadyExistsException(string? message)
        : base(message)
    {
    }

    public QueueAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected QueueAlreadyExistsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class QueueIsDynamicException : Exception
{
    public string QueueName { get; set; }

    public QueueIsDynamicException(string queueName)
        : this(message: null, queueName)
    {
    }

    public QueueIsDynamicException(string? message, string queueName)
        : this(message, queueName, innerException: null)
    {
    }

    public QueueIsDynamicException(string? message, string queueName, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected QueueIsDynamicException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

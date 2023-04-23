using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class QueueIsFullException : Exception
{
    public string QueueName { get; set; }

    public QueueIsFullException(string queueName)
        : this(message: null, queueName)
    {
    }

    public QueueIsFullException(string? message, string queueName)
        : this(message, queueName, innerException: null)
    {
    }

    public QueueIsFullException(string? message, string queueName, Exception? innerException)
        : base(message, innerException)
    {
        QueueName = queueName;
    }

    protected QueueIsFullException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class QueueNameIsTooLongException : Exception
{
    public string QueueName { get; set; }

    public QueueNameIsTooLongException(string queueName)
        : this(queueName, null)
    {
    }

    public QueueNameIsTooLongException(string queueName, string? message)
        : this(queueName, message, null)
    {
    }

    public QueueNameIsTooLongException(string queueName, string? message, Exception? innerException)
        : base(message, innerException)
    {
        QueueName = queueName;
    }

    protected QueueNameIsTooLongException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

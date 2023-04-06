using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class PositionIsReservedException : Exception
{
    public string QueueName { get; set; }

    public PositionIsReservedException(string queueName)
        : this(message: null, queueName)
    {
    }

    public PositionIsReservedException(string? message, string queueName)
        : this(message, queueName, innerException: null)
    {
    }

    public PositionIsReservedException(string? message, string queueName, Exception? innerException)
        : base(message, innerException)
    {
        QueueName = queueName;
    }

    protected PositionIsReservedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

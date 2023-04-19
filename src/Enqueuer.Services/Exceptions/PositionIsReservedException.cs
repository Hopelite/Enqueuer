using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class PositionIsReservedException : Exception
{
    public string QueueName { get; set; }

    public int Position { get; set; }

    public PositionIsReservedException(string queueName, int position)
        : this(message: null, queueName, position)
    {
    }

    public PositionIsReservedException(string? message, string queueName, int position)
        : this(message, queueName, position, innerException: null)
    {
    }

    public PositionIsReservedException(string? message, string queueName, int position, Exception? innerException)
        : base(message, innerException)
    {
        QueueName = queueName;
        Position = position;
    }

    protected PositionIsReservedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

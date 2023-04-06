using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class UserAlreadyParticipatesException : Exception
{
    public string QueueName { get; set; }

    public UserAlreadyParticipatesException(string queueName)
        : this(message: null, queueName)
    {
    }

    public UserAlreadyParticipatesException(string? message, string queueName)
        : this(message, queueName, innerException: null)
    {
    }

    public UserAlreadyParticipatesException(string? message, string queueName, Exception? innerException)
        : base(message, innerException)
    {
        QueueName = queueName;
    }

    protected UserAlreadyParticipatesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

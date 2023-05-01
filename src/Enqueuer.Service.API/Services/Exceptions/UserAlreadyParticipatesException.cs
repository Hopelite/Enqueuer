using System;
using System.Runtime.Serialization;

namespace Enqueuer.Service.API.Services.Exceptions;

[Serializable]
public class UserAlreadyParticipatesException : Exception
{
    public UserAlreadyParticipatesException()
    {
    }

    public UserAlreadyParticipatesException(string? message)
        : base(message)
    {
    }

    public UserAlreadyParticipatesException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected UserAlreadyParticipatesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

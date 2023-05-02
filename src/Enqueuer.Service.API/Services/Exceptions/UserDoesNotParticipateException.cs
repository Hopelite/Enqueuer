using System;
using System.Runtime.Serialization;

namespace Enqueuer.Service.API.Services.Exceptions;

[Serializable]
public class UserDoesNotParticipateException : Exception
{
    public UserDoesNotParticipateException()
    {
    }

    public UserDoesNotParticipateException(string? message)
        : base(message)
    {
    }

    public UserDoesNotParticipateException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected UserDoesNotParticipateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

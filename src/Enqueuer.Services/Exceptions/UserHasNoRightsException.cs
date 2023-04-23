using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class UserHasNoRightsException : Exception
{
    public UserHasNoRightsException()
    {
    }

    public UserHasNoRightsException(string? message)
        : base(message)
    {
    }

    public UserHasNoRightsException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected UserHasNoRightsException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}

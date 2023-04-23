using System;
using System.Runtime.Serialization;

namespace Enqueuer.Services.Exceptions;

[Serializable]
public class InvalidMemberPositionException : Exception
{
    public InvalidMemberPositionException()
    {
    }

    public InvalidMemberPositionException(string? message)
        : base(message)
    {
    }

    public InvalidMemberPositionException(string? message, Exception? innerException) 
        : base(message, innerException)
    {
    }

    protected InvalidMemberPositionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

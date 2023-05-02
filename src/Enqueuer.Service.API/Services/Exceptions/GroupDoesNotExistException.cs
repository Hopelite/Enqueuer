using System;
using System.Runtime.Serialization;

namespace Enqueuer.Service.API.Services.Exceptions;

[Serializable]
public class GroupDoesNotExistException : Exception
{
    public GroupDoesNotExistException()
    {
    }

    public GroupDoesNotExistException(string? message)
        : base(message)
    {
    }

    public GroupDoesNotExistException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected GroupDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

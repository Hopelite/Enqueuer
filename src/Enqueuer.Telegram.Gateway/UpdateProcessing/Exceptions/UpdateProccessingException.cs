using System;
using System.Runtime.Serialization;

namespace Enqueuer.Telegram.Gateway.UpdateProcessing.Exceptions;

[Serializable]
public class UpdateProccessingException : Exception
{
    public UpdateProccessingException()
    {
    }

    public UpdateProccessingException(string? message)
        : base(message)
    {
    }

    public UpdateProccessingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected UpdateProccessingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

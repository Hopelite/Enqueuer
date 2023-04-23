using System;

namespace Enqueuer.Telegram.Core.Exceptions;

public class OutdatedCallbackException : Exception
{
    public OutdatedCallbackException()
    {
    }

    public OutdatedCallbackException(string message)
        : base(message)
    {
    }

    public OutdatedCallbackException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

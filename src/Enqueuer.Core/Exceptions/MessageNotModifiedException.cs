using System;
using Telegram.Bot.Exceptions;

namespace Enqueuer.Core.Exceptions;

public class MessageNotModifiedException : ApiRequestException
{
    public MessageNotModifiedException(string message, int errorCode)
        : base(message, errorCode)
    {
    }

    public MessageNotModifiedException(string message, int errorCode, Exception innerException)
        : base(message, errorCode, innerException)
    {
    }
}

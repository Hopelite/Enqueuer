using System;
using Telegram.Bot.Exceptions;

namespace Enqueuer.Data.Exceptions
{
    public class MessageNotModifiedException : ApiRequestException
    {
        private const int BadRequestErrorCode = 400;

        public MessageNotModifiedException(string message)
            : base(message, BadRequestErrorCode)
        {
        }

        public MessageNotModifiedException(string message, Exception innerException)
            : base(message, BadRequestErrorCode, innerException)
        {
        }
    }
}

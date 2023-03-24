﻿using System;
using Telegram.Bot.Exceptions;

namespace Enqueuer.Data.Exceptions;

public class NotFoundException : ApiRequestException
{
    public NotFoundException(string message, int errorCode)
        : base(message, errorCode)
    {
    }

    public NotFoundException(string message, int errorCode, Exception innerException)
        : base(message, errorCode, innerException)
    {
    }
}

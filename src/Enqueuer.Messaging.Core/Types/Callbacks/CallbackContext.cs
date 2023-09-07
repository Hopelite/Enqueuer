using System;
using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types;

namespace Enqueuer.Messaging.Core.Types.Callbacks;

public class CallbackContext
{
    public static bool TryCreate(CallbackQuery callbackQuery, [NotNullWhen(returnValue: true)] out CallbackContext? callbackContext)
    {
        throw new NotImplementedException();
    }
}

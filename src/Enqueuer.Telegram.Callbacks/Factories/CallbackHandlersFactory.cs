using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;
using Enqueuer.Messaging.Core.Constants;
using Microsoft.Extensions.DependencyInjection;
using Enqueuer.Messaging.Core.Types.Callbacks;

namespace Enqueuer.Telegram.Callbacks.Factories;

public class CallbackHandlersFactory : ICallbackHandlersFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CallbackHandlersFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool TryCreateCallbackHandler(Messaging.Core.Types.Callbacks.CallbackContext callbackContext, [NotNullWhen(true)] out ICallbackHandler? callbackHandler)
    {
        callbackHandler = null;
        if (callbackContext == null || callbackContext.CallbackData == null)
        {
            return false;
        }

        return TryCreateCallbackHandler(callbackContext.CallbackData.Command, out callbackHandler);
    }

    private bool TryCreateCallbackHandler(string command, out ICallbackHandler? callbackHandler)
    {
        callbackHandler = command switch
        {
            CallbackCommands.EnqueueMeCommand => _serviceProvider.GetRequiredService<EnqueueMeCallbackHandler>(),
            CallbackCommands.GetChatCommand => _serviceProvider.GetRequiredService<GetChatCallbackHandler>(),
            CallbackCommands.GetQueueCommand => _serviceProvider.GetRequiredService<GetQueueCallbackHandler>(),
            CallbackCommands.ListChatsCommand => _serviceProvider.GetRequiredService<ListChatsCallbackHandler>(),
            CallbackCommands.EnqueueCommand => _serviceProvider.GetRequiredService<EnqueueCallbackHandler>(),
            CallbackCommands.EnqueueAtCommand => _serviceProvider.GetRequiredService<EnqueueAtCallbackHandler>(),
            CallbackCommands.DequeueMeCommand => _serviceProvider.GetRequiredService<DequeueMeCallbackHandler>(),
            CallbackCommands.RemoveQueueCommand => _serviceProvider.GetRequiredService<RemoveQueueCallbackHandler>(),
            CallbackCommands.SwitchQueueDynamicCommand => _serviceProvider.GetRequiredService<SwitchQueueCallbackHandler>(),
            CallbackCommands.ExchangePositionsCommand => _serviceProvider.GetRequiredService<SwapPositionsCallbackHandler>(),
            _ => null
        };

        return callbackHandler != null;
    }
}

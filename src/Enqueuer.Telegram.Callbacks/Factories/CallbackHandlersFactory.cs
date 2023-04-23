using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Telegram.Callbacks.CallbackHandlers;
using Enqueuer.Telegram.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Telegram.Callbacks.Factories;

public class CallbackHandlersFactory : ICallbackHandlersFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CallbackHandlersFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool TryCreateCallbackHandler(Callback callback, [NotNullWhen(returnValue: true)] out ICallbackHandler? callbackHandler)
    {
        callbackHandler = null;
        if (callback == null || callback.CallbackData == null)
        {
            return false;
        }

        return TryCreateCallbackHandler(callback.CallbackData.Command, out callbackHandler);
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

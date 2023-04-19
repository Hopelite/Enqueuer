using System;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Callbacks.Factories;

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
            CallbackConstants.EnqueueMeCommand => _serviceProvider.GetRequiredService<EnqueueMeCallbackHandler>(),
            CallbackConstants.GetChatCommand => _serviceProvider.GetRequiredService<GetChatCallbackHandler>(),
            CallbackConstants.GetQueueCommand => _serviceProvider.GetRequiredService<GetQueueCallbackHandler>(),
            CallbackConstants.ListChatsCommand => _serviceProvider.GetRequiredService<ListChatsCallbackHandler>(),
            CallbackConstants.EnqueueCommand => _serviceProvider.GetRequiredService<EnqueueCallbackHandler>(),
            CallbackConstants.EnqueueAtCommand => _serviceProvider.GetRequiredService<EnqueueAtCallbackHandler>(),
            CallbackConstants.DequeueMeCommand => _serviceProvider.GetRequiredService<DequeueMeCallbackHandler>(),
            CallbackConstants.RemoveQueueCommand => _serviceProvider.GetRequiredService<RemoveQueueCallbackHandler>(),
            CallbackConstants.SwitchQueueDynamicCommand => _serviceProvider.GetRequiredService<SwitchQueueCallbackHandler>(),
            CallbackConstants.ExchangePositionsCommand => _serviceProvider.GetRequiredService<SwapPositionsCallbackHandler>(),
            _ => null
        };

        return callbackHandler != null;
    }
}

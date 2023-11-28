using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Telegram.Callbacks.Factories;

namespace Enqueuer.Telegram.Callbacks;

public class CallbackDistributor : ICallbackDistributor
{
    private readonly ICallbackHandlersFactory _callbackHandlersFactory;

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
    }

    public Task DistributeAsync(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (_callbackHandlersFactory.TryCreateCallbackHandler(callbackContext, out var callbackHandler))
        {
            return callbackHandler.HandleAsync(callbackContext, cancellationToken);
        }

        return Task.CompletedTask;
    }
}

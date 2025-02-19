using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Telegram.Callbacks;
using Enqueuer.Telegram.Messages;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.UpdateHandling;

public class UpdateHandler : IUpdateHandler
{
    private readonly IMessageDistributor _messageDistributor;
    private readonly ICallbackDistributor _callbackDistributor;

    public UpdateHandler(IMessageDistributor messageDistributor, ICallbackDistributor callbackDistributor)
    {
        _messageDistributor = messageDistributor;
        _callbackDistributor = callbackDistributor;
    }

    public Task HandleAsync(Update update)
    {
        if (update?.Type == UpdateType.Message && MessageContext.TryCreate(update.Message, out var messageContext))
        {
            return _messageDistributor.DistributeAsync(messageContext, CancellationToken.None);
        }
        else if (update?.Type == UpdateType.CallbackQuery)
        {
            return _callbackDistributor.DistributeAsync(update.CallbackQuery, CancellationToken.None);
        }

        return Task.CompletedTask;
    }
}

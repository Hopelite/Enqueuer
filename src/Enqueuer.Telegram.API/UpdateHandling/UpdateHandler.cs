using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks;
using Enqueuer.Messages;
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
        if (update?.Type == UpdateType.Message)
        {
            return _messageDistributor.DistributeAsync(update.Message, CancellationToken.None);
        }
        else if (update?.Type == UpdateType.CallbackQuery)
        {
            return _callbackDistributor.DistributeAsync(update.CallbackQuery, CancellationToken.None);
        }

        return Task.CompletedTask;
    }
}

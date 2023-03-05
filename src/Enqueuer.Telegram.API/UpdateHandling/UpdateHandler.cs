using System.Threading.Tasks;
using Enqueuer.Callbacks;
using Enqueuer.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.UpdateHandling;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IMessageDistributor _messageDistributor;
    private readonly ICallbackDistributor _callbackDistributor;

    public UpdateHandler(ITelegramBotClient telegramBotClient, IMessageDistributor messageDistributor, ICallbackDistributor callbackDistributor)
    {
        _telegramBotClient = telegramBotClient;
        _messageDistributor = messageDistributor;
        _callbackDistributor = callbackDistributor;
    }

    public Task HandleAsync(Update update)
    {
        if (update?.Type == UpdateType.Message)
        {
            return _messageDistributor.DistributeAsync(update.Message);
        }
        else if (update?.Type == UpdateType.CallbackQuery)
        {
            return _callbackDistributor.DistributeCallbackAsync(_telegramBotClient, update.CallbackQuery);
        }

        return Task.CompletedTask;
    }
}

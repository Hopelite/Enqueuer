using System.Threading.Tasks;
using Enqueuer.Bot.Callbacks;
using Enqueuer.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Bot
{
    /// <inheritdoc/>
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient telegramBotClient;
        private readonly IMessageDistributor messageDistributor;
        private readonly ICallbackDistributor callbackDistributor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateHandler"/> class.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="messageDistributor"><see cref="IMessageDistributor"/> to handle incoming messages.</param>
        /// <param name="callbackDistributor"><see cref="ICallbackDistributor"/> to handler incoming callback.</param>
        public UpdateHandler(ITelegramBotClient telegramBotClient, IMessageDistributor messageDistributor, ICallbackDistributor callbackDistributor)
        {
            this.telegramBotClient = telegramBotClient;
            this.messageDistributor = messageDistributor;
            this.callbackDistributor = callbackDistributor;
        }

        /// <inheritdoc/>
        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                await this.messageDistributor.DistributeMessageAsync(telegramBotClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await this.callbackDistributor.DistributeCallbackAsync(telegramBotClient, update.CallbackQuery);
            }
        }
    }
}
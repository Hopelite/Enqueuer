using System;
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
        private readonly Lazy<IMessageDistributor> messageDistributor;
        private readonly Lazy<ICallbackDistributor> callbackDistributor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateHandler"/> class.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="messageDistributor">Lazy <see cref="IMessageDistributor"/> to handle incoming messages.</param>
        /// <param name="callbackDistributor">Lazy <see cref="ICallbackDistributor"/> to handler incoming callback.</param>
        public UpdateHandler(ITelegramBotClient telegramBotClient, Lazy<IMessageDistributor> messageDistributor, Lazy<ICallbackDistributor> callbackDistributor)
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
                await this.messageDistributor.Value.DistributeMessageAsync(telegramBotClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await this.callbackDistributor.Value.DistributeCallbackAsync(telegramBotClient, update.CallbackQuery);
            }
        }
    }
}
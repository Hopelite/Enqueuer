using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Bot
{
    /// <inheritdoc/>
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient botClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateHandler"/> class.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.</param>
        public UpdateHandler(ITelegramBotClient telegramBotClient)
        {
            this.botClient = telegramBotClient;
        }

        /// <inheritdoc/>
        public Task HadnleUpdateAsync(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
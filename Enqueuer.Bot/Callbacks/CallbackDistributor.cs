using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Enqueuer.Bot.Callbacks.CallbackHandlers;
using Enqueuer.Bot.Extensions;
using Enqueuer.Bot.Factories;
using Microsoft.Extensions.Logging;

namespace Enqueuer.Bot.Callbacks
{
    /// <inheritdoc/>
    public class CallbackDistributor : ICallbackDistributor
    {
        private readonly SortedDictionary<string, ICallbackHandler> callbackHandlers;
        private readonly ILogger<ICallbackDistributor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackDistributor"/> class and adds <see cref="ICallbackHandler"/> using <paramref name="callbackHandlersFactory"/>.
        /// </summary>
        /// <param name="callbackHandlersFactory"><see cref="ICallbackHandlersFactory"/> which provides distibutor with <see cref="ICallbackHandler"/>.</param>
        /// <param name="logger"><see cref="ILogger"/> to log info.</param>
        public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, ILogger<ICallbackDistributor> logger)
        {
            this.callbackHandlers = new SortedDictionary<string, ICallbackHandler>(
                callbackHandlersFactory
                .CreateCallbackHandlers()
                .ToDictionary(callbackHandler => callbackHandler.Command));
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task DistributeCallbackAsync(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery)
        {
            var command = callbackQuery.Data?.SplitToWords()[0];
            if (command is not null && this.callbackHandlers.TryGetValue(command, out ICallbackHandler callbackHandler))
            {
                var sentMessage = await callbackHandler.HandleCallbackAsync(telegramBotClient, callbackQuery);
                this.logger.LogInformation($"Sent message '{sentMessage.Text}' on user's callback to {sentMessage.Chat.Title ?? "@" + sentMessage.Chat.Username}.");
            }
        }
    }
}

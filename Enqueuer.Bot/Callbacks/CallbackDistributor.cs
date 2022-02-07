using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Enqueuer.Bot.Callbacks.CallbackHandlers;
using Enqueuer.Bot.Extensions;
using Enqueuer.Bot.Factories;

namespace Enqueuer.Bot.Callbacks
{
    /// <inheritdoc/>
    public class CallbackDistributor : ICallbackDistributor
    {
        private readonly SortedDictionary<string, ICallbackHandler> callbackHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackDistributor"/> class and adds <see cref="ICallbackHandler"/> using <paramref name="callbackHandlersFactory"/>.
        /// </summary>
        /// <param name="callbackHandlersFactory"><see cref="ICallbackHandlersFactory"/> which provides distibutor with <see cref="ICallbackHandler"/>.</param>
        public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory)
        {
            this.callbackHandlers = new SortedDictionary<string, ICallbackHandler>(
                callbackHandlersFactory
                .CreateCallbackHandlers()
                .ToDictionary(callbackHandler => callbackHandler.Command));
        }

        /// <inheritdoc/>
        public async Task DistributeCallbackAsync(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery)
        {
            var command = callbackQuery.Data?.SplitToWords()[0];
            if (command is not null && this.callbackHandlers.TryGetValue(command, out ICallbackHandler callbackHandler))
            {
                await callbackHandler.HandleCallbackAsync(telegramBotClient, callbackQuery);
            }
        }
    }
}

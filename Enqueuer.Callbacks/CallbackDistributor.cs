using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Callbacks.Factories;
using Microsoft.Extensions.Logging;
using Enqueuer.Data;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Utilities.Configuration;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks
{
    /// <inheritdoc/>
    public class CallbackDistributor : ICallbackDistributor
    {
        private readonly SortedDictionary<string, ICallbackHandler> callbackHandlers;
        private readonly ILogger<ICallbackDistributor> logger;
        private readonly IDataDeserializer dataDeserializer;
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackDistributor"/> class and adds <see cref="ICallbackHandler"/> using <paramref name="callbackHandlersFactory"/>.
        /// </summary>
        /// <param name="callbackHandlersFactory"><see cref="ICallbackHandlersFactory"/> which provides distibutor with <see cref="ICallbackHandler"/>.</param>
        /// <param name="logger"><see cref="ILogger"/> to log info.</param>
        /// <param name="dataDeserializer"><see cref="IDataDeserializer"/> to deserialize callback data.</param>
        /// <param name="botConfiguration"><see cref="IBotConfiguration"/> to rely on.</param>
        public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, ILogger<ICallbackDistributor> logger, IDataDeserializer dataDeserializer, IBotConfiguration botConfiguration)
        {
            this.callbackHandlers = new SortedDictionary<string, ICallbackHandler>(
                callbackHandlersFactory
                .CreateCallbackHandlers()
                .ToDictionary(callbackHandler => callbackHandler.Command));
            this.logger = logger;
            this.dataDeserializer = dataDeserializer;
            this.botConfiguration = botConfiguration;
        }

        /// <inheritdoc/>
        public async Task DistributeCallbackAsync(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery)
        {
            var callbackData = this.dataDeserializer.Deserialize<CallbackData>(callbackQuery.Data);
            if (callbackData.Command is not null && this.callbackHandlers.TryGetValue(callbackData.Command, out ICallbackHandler callbackHandler))
            {
                try
                {
                    var sentMessage = await callbackHandler.HandleCallbackAsync(telegramBotClient, callbackQuery, callbackData);
                    this.logger.LogInformation($"Sent message '{sentMessage.Text}' on user's callback to {sentMessage.Chat.Title ?? "@" + sentMessage.Chat.Username}.");
                }
                catch (Exception ex)
                {
                    await telegramBotClient.SendTextMessageAsync(
                        this.botConfiguration.DevelomentChatId,
                        $"Exception thrown while handling callback '{callbackQuery.Data}' from {callbackQuery.From.Username ?? callbackQuery.From.FirstName + callbackQuery.From.LastName ?? string.Empty}\n"
                        + $"<b>Exception message:</b> {ex.Message}\n"
                        + $"<b>Stack trace:</b> {ex.StackTrace}",
                        ParseMode.Html);
                }
            }
        }
    }
}

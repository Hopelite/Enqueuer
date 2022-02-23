using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Messages.Factories;
using Enqueuer.Messages.MessageHandlers;
using Enqueuer.Data.Configuration;
using Enqueuer.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Messages
{
    /// <inheritdoc/>
    public class MessageDistributor : IMessageDistributor
    {
        private readonly SortedDictionary<string, IMessageHandler> messageHandlers;
        private readonly ILogger<IMessageDistributor> logger;
        private readonly IBotConfiguration botConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDistributor"/> class and adds message handlers using <paramref name="messageHandlersFactory"/>.
        /// </summary>
        /// <param name="messageHandlersFactory"><see cref="IMessageHandlersFactory"/> which provides distibutor with <see cref="IMessageHandler"/>.</param>
        /// <param name="logger"><see cref="ILogger"/> to log with.</param>
        /// <param name="botConfiguration"><see cref="IBotConfiguration"/> to rely on.</param>
        public MessageDistributor(IMessageHandlersFactory messageHandlersFactory, ILogger<IMessageDistributor> logger, IBotConfiguration botConfiguration)
        {
            this.messageHandlers = new SortedDictionary<string, IMessageHandler>(
                messageHandlersFactory
                .CreateMessageHandlers()
                .ToDictionary(messageHandler => messageHandler.Command));
            this.logger = logger;
            this.botConfiguration = botConfiguration;
        }

        /// <inheritdoc/>
        public async Task DistributeMessageAsync(ITelegramBotClient telegramBotClient, Message message)
        {
            var command = message.Text?.SplitToWords()[0];
            if (command is not null)
            {
                var messageHandler = this.messageHandlers.FirstOrDefault(pair => command.Contains(pair.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
                if (messageHandler is not null)
                {
                    try
                    {
                        var sentMessage = await messageHandler.HandleMessageAsync(telegramBotClient, message);
                        this.logger.LogInformation("Sent message '{Text}' to {To}", sentMessage.Text, sentMessage.Chat.Title ?? "@" + sentMessage.Chat.Username);
                    }
                    catch (Exception ex)
                    {
                        await telegramBotClient.SendTextMessageAsync(
                            this.botConfiguration.DevelomentChatId,
                            $"Exception thrown while handling '{message.Text}' from {message.From.Username ?? message.From.FirstName + message.From.LastName ?? string.Empty}\n"
                            + $"Exception message: {ex.Message}\n"
                            + $"Stack trace: {ex.StackTrace}");
                    }
                }
            }
        }
    }
}

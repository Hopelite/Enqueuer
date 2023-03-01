using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <summary>
    /// Contains basic implementation for message handlers.
    /// </summary>
    public abstract class MessageHandlerBase : IMessageHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MessageHandlerBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task HandleAsync(Message message)
        {
            using var scope = _scopeFactory.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            return HandleImplementationAsync(scope, botClient, message);
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract Task HandleImplementationAsync(IServiceScope serviceScope, ITelegramBotClient botClient, Message message);
    }
}

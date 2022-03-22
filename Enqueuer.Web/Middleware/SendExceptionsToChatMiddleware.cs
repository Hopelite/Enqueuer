using System;
using System.Threading.Tasks;
using Enqueuer.Data.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Enqueuer.Web.Middleware
{
    /// <summary>
    /// Catches all exceptions occurring during request handling and logs the by sending them to telegram chat.
    /// </summary>
    public class SendExceptionsToChatMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendExceptionsToChatMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate to pass request to.</param>
        public SendExceptionsToChatMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes <see cref="SendExceptionsToChatMiddleware"/> and starts catching exceptions.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> to pass down by pipeline.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception ex)
            {
                var botClient = context.RequestServices.GetService<ITelegramBotClient>();
                var botConfiguration = context.RequestServices.GetService<IBotConfiguration>();
                await botClient.SendTextMessageAsync(
                    botConfiguration.DevelomentChatId,
                    $"Exception thrown during application work.\n"
                    + $"Exception message: {ex.Message}\n"
                    + $"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

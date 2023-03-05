using System;
using System.Threading.Tasks;
using Enqueuer.Data.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Enqueuer.Telegram.Middleware;

/// <summary>
/// Catches all exceptions occurred during the request handling and logs them by sending to a telegram chat.
/// </summary>
public class SendExceptionsToChatMiddleware // TODO: replace with LogProvider
{
    private readonly RequestDelegate _next;

    public SendExceptionsToChatMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes <see cref="SendExceptionsToChatMiddleware"/> and starts catching exceptions.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> to pass down by pipeline.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            //var botClient = context.RequestServices.GetService<ITelegramBotClient>();
            //var botConfiguration = context.RequestServices.GetService<IBotConfiguration>();

            //try
            //{
            //    await botClient.SendTextMessageAsync(
            //        botConfiguration.DevelopmentChatId,
            //        $"Exception thrown during application work.\n"
            //        + $"Exception message: {ex.Message}\n"
            //        + $"Stack trace: {ex.StackTrace}");
            //}
            //catch (Exception)
            //{
            //}
        }
    }
}

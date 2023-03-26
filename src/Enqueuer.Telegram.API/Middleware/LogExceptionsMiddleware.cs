using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enqueuer.Telegram.Middleware;

/// <summary>
/// Catches all exceptions occurred during the request handling and logs them by sending to a telegram chat.
/// </summary>
public class LogExceptionsMiddleware // TODO: replace with LogProvider
{
    private readonly RequestDelegate _next;

    public LogExceptionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes <see cref="LogExceptionsMiddleware"/> and starts catching exceptions.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> to pass down by pipeline.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<LogExceptionsMiddleware>>();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}

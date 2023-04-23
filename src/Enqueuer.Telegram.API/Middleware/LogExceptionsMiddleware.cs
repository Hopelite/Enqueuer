using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enqueuer.Telegram.Middleware;

/// <summary>
/// Logs all thrown exceptions.
/// </summary>
public class LogExceptionsMiddleware
{
    private readonly RequestDelegate _next;

    public LogExceptionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<LogExceptionsMiddleware>>();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception was thrown during the application work.");
        }
    }
}

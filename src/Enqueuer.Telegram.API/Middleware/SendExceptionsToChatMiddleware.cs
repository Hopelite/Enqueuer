using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        catch (Exception)
        {
        }
    }
}

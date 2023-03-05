using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Messages.MessageHandlers;

/// <summary>
/// Contains basic implementation for message handlers.
/// </summary>
public abstract class MessageHandlerBase : IMessageHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected MessageHandlerBase(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task HandleAsync(Message message)
    {
        using var scope = _scopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        return HandleAsyncImplementation(scope.ServiceProvider, botClient, message);
    }

    /// <summary>
    /// Contains the implementation of <paramref name="message"/> handling.
    /// </summary>
    protected abstract Task HandleAsyncImplementation(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message);
}

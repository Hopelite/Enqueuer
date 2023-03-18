using Enqueuer.Callbacks;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Data.Configuration;
using Enqueuer.Data.Exceptions;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages;
using Enqueuer.Messages.Factories;
using Enqueuer.Persistence;
using Enqueuer.Telegram.API.Extensions;
using Enqueuer.Telegram.Configuration;
using Enqueuer.Telegram.Extensions;
using Enqueuer.Telegram.Middleware;
using Enqueuer.Telegram.UpdateHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.API;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();

        var botConfiguration = app.Services.GetRequiredService<IBotConfiguration>();
        app.MapPost($"/bot{botConfiguration.AccessToken}", async (HttpContext context) =>
        {
            if (context.Request.HasJsonContentType())
            {
                var update = context.DeserializeBody<Update>();
                var handler = context.RequestServices.GetRequiredService<IUpdateHandler>();
                await handler.HandleAsync(update);
            }
        });

        app.UseMiddleware<SendExceptionsToChatMiddleware>();

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<EnqueuerContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("Enqueuer.Telegram.API")));

        builder.Services.AddTransient<IBotConfiguration, BotConfiguration>(services => 
        {
                var configuration = services.GetRequiredService<IConfiguration>();
                return configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        });

        builder.Services.AddScoped<IMessageDistributor, MessageDistributor>();
        builder.Services.AddTransient<IMessageHandlersFactory, MessageHandlersFactory>();
        builder.Services.ConfigureMessageHandlers();

        builder.Services.AddScoped<ICallbackDistributor, CallbackDistributor>();
        builder.Services.AddTransient<ICallbackHandlersFactory, CallbackHandlersFactory>();
        builder.Services.ConfigureCallbackHandlers();

        builder.Services.AddTransient<IMessageProvider, InMemoryTextProvider>();

        builder.Services.ConfigureSerialization()
            .ConfigureServices();

        builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();

        builder.Services.AddHttpClient(nameof(TelegramBotClient))
            .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
            {
                var botConfiguration = serviceProvider.GetService<IBotConfiguration>();
                return new TelegramBotClient(botConfiguration.AccessToken, httpClient)
                {
                    ExceptionsParser = new TelegramExceptionsParser()
                };
            });
    }
}

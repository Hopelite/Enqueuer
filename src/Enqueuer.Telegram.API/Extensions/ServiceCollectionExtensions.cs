using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Telegram.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddTransient<IGroupService, GroupService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IQueueService, QueueService>();

        return services;
    }

    public static IServiceCollection ConfigureSerialization(this IServiceCollection services)
    {
        services.AddTransient<IDataSerializer, JsonDataSerializer>();
        services.AddTransient<IDataDeserializer, JsonDataDeserializer>();

        return services;
    }

    public static IServiceCollection ConfigureCallbackHandlers(this IServiceCollection services)
    {
        services.AddScoped<EnqueueMeCallbackHandler>();
        services.AddScoped<GetChatCallbackHandler>();
        services.AddScoped<GetQueueCallbackHandler>();
        services.AddScoped<ListChatsCallbackHandler>();
        services.AddScoped<EnqueueCallbackHandler>();
        services.AddScoped<EnqueueAtCallbackHandler>();
        services.AddScoped<DequeueMeCallbackHandler>();
        services.AddScoped<RemoveQueueCallbackHandler>();
        services.AddScoped<SwitchQueueDynamicHandler>();

        return services;
    }
}

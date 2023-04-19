using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Core.Serialization;
using Enqueuer.Messages.MessageHandlers;
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
        services.AddTransient<ICallbackDataSerializer, JsonCallbackDataSerializer>();
        services.AddTransient<ICallbackDataDeserializer, JsonCallbackDataDeserializer>();

        return services;
    }

    public static IServiceCollection ConfigureMessageHandlers(this IServiceCollection services)
    {
        services.AddScoped<StartMessageHandler>();
        services.AddScoped<HelpMessageHandler>();
        services.AddScoped<QueueMessageHandler>();
        services.AddScoped<EnqueueMessageHandler>();
        services.AddScoped<DequeueMessageHandler>();
        services.AddScoped<CreateQueueMessageHandler>();
        services.AddScoped<RemoveQueueMessageHandler>();

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
        services.AddScoped<SwitchQueueCallbackHandler>();
        services.AddScoped<SwapPositionsCallbackHandler>();

        return services;
    }
}

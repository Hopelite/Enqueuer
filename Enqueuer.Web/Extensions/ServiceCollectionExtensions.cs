using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Web.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures project repositories.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> to configure repositories by.</param>
        /// <returns><see cref="IServiceCollection"/> with configured repositories.</returns>
        public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Chat>, Repository<Chat>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Queue>, Repository<Queue>>();
            services.AddScoped<IRepository<UserInQueue>, Repository<UserInQueue>>();

            return services;
        }

        /// <summary>
        /// Configures project services.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> to configure services by.</param>
        /// <returns><see cref="IServiceCollection"/> with configured services.</returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IQueueService, QueueService>();
            services.AddTransient<IUserInQueueService, UserInQueueService>();

            return services;
        }

        /// <summary>
        /// Configures serialization and deserialization.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> to configure serialization by.</param>
        /// <returns><see cref="IServiceCollection"/> with configured serialization.</returns>
        public static IServiceCollection ConfigureSerialization(this IServiceCollection services)
        {
            services.AddTransient<IDataSerializer, JsonDataSerializer>();
            services.AddTransient<IDataDeserializer, JsonDataDeserializer>();

            return services;
        }
    }
}

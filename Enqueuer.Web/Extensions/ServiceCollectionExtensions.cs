using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Chat>, Repository<Chat>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Queue>, Repository<Queue>>();
            services.AddScoped<IRepository<UserInQueue>, Repository<UserInQueue>>();

            return services;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IQueueService, QueueService>();
            services.AddTransient<IUserInQueueService, UserInQueueService>();

            return services;
        }
    }
}

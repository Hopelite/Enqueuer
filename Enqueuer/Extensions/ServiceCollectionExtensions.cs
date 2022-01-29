using System;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Chat>, Repository<Chat>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Queue>, Repository<Queue>>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IQueueService, QueueService>();

            return services;
        }
    }
}

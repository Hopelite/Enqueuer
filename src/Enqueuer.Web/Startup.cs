using System;
using Enqueuer.Callbacks;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Messages;
using Enqueuer.Messages.Factories;
using Enqueuer.Persistence;
using Enqueuer.Data.Configuration;
using Enqueuer.Web.Configuration;
using Enqueuer.Web.Extensions;
using Enqueuer.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Enqueuer.Web
{
    /// <summary>
    /// Application startup class.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configures application service.
        /// </summary>
        /// <param name="services">Collection of services to add services to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EnqueuerContext>(options =>
                   options.UseSqlServer(_configuration.GetConnectionString("Default")),
                   contextLifetime: ServiceLifetime.Transient,
                   optionsLifetime: ServiceLifetime.Singleton);

            services.AddTransient<IBotConfiguration, BotConfiguration>();

            services.AddHttpClient("Webhook")
                .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
                {
                    var botConfiguration = serviceProvider.GetService<IBotConfiguration>();
                    return new TelegramBotClient(botConfiguration.AccessToken, httpClient);
                });

            services.ConfigureSerialization();
            services.ConfigureRepositories();
            services.ConfigureServices();
            services.AddTransient<IMessageHandlersFactory, MessageHandlersFactory>();
            services.AddScoped<IMessageDistributor, MessageDistributor>();
            services.AddScoped(provider => new Lazy<IMessageDistributor>(provider.GetService<IMessageDistributor>));
            services.AddTransient<ICallbackHandlersFactory, CallbackHandlersFactory>();
            services.AddScoped<ICallbackDistributor, CallbackDistributor>();
            services.AddScoped(provider => new Lazy<ICallbackDistributor>(provider.GetService<ICallbackDistributor>));
            services.AddScoped<IUpdateHandler, UpdateHandler>();

            services.AddControllers()
                .AddNewtonsoftJson();
        }

        /// <summary>
        /// Configure request pipeline.
        /// </summary>
        /// <param name="app">Application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<SendExceptionsToChatMiddleware>();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var botConfiguration = app.ApplicationServices.GetService<IBotConfiguration>();
                endpoints.MapControllerRoute(name: "Webhook",
                             pattern: $"bot{botConfiguration.AccessToken}",
                             new { controller = "Bot", action = "Post" });
                endpoints.MapControllers();
            });
        }
    }
}
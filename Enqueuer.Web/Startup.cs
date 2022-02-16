using System;
using Enqueuer.Bot;
using Enqueuer.Callbacks;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Messages;
using Enqueuer.Messages.Factories;
using Enqueuer.Persistence;
using Enqueuer.Utilities.Configuration;
using Enqueuer.Web.Configuration;
using Enqueuer.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Enqueuer.Web
{
    /// <summary>
    /// Application startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            this.BotConfiguration = new BotConfiguration(this.Configuration);
        }

        /// <summary>
        /// Gets application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets bot configuration.
        /// </summary>
        public IBotConfiguration BotConfiguration { get; }

        /// <summary>
        /// Configures application service.
        /// </summary>
        /// <param name="services">Collection of services to add services to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EnqueuerContext>(options =>
                   options.UseSqlServer(this.Configuration.GetConnectionString("Default")),
                   contextLifetime: ServiceLifetime.Transient,
                   optionsLifetime: ServiceLifetime.Singleton);

            services.AddTransient(_ => this.BotConfiguration);

            services.AddHttpClient("Webhook")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(BotConfiguration.AccessToken, httpClient));

            services.ConfigureRepositories();
            services.ConfigureServices();
            services.AddTransient<IMessageHandlersFactory, MessageHandlersFactory>();
            services.AddTransient<IPrivateChatMessageHandlersFactory, PrivateChatMessageHandlersFactory>();
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
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var accessToken = this.BotConfiguration.AccessToken;
                endpoints.MapControllerRoute(name: "Webhook",
                             pattern: $"bot{BotConfiguration.AccessToken}",
                             new { controller = "Bot", action = "Post" });
                endpoints.MapControllers();
            });
        }
    }
}
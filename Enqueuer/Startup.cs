using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Bot;
using Enqueuer.Bot.Factories;
using Enqueuer.Bot.Messages;
using Enqueuer.Persistence;
using Enqueuer.Web.Configuration;
using Enqueuer.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Enqueuer.Web
{
    public class Startup
    {
        public Startup()
        {
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            this.BotConfiguration = new BotConfiguration(this.Configuration);
        }

        public IConfiguration Configuration { get; }

        public IBotConfiguration BotConfiguration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EnqueuerContext>(options =>
                   options.UseSqlServer(this.Configuration.GetConnectionString("Default")),
                   contextLifetime: ServiceLifetime.Transient,
                   optionsLifetime: ServiceLifetime.Singleton);

            services.AddHttpClient("Webhook")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(BotConfiguration.AccessToken, httpClient));

            services.ConfigureServices();
            services.AddTransient<IMessageHandlersFactory, MessageHandlersFactory>();
            services.AddScoped<IMessageDistributor, MessageDistributor>();
            services.AddScoped<IUpdateHandler, UpdateHandler>();

            services.AddControllers()
                .AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
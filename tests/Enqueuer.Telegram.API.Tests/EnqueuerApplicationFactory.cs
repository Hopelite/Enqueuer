using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Data.Configuration;
using Enqueuer.Data.Exceptions;
using Enqueuer.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Telegram.Bot;

namespace Enqueuer.Telegram.API.Tests;

internal class EnqueuerApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ITelegramBotClient> TelegramClientMock { get; } = new Mock<ITelegramBotClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.Single(d => d.ServiceType == typeof(EnqueuerContext));
            services.Remove(dbContextDescriptor);
            services.AddDbContext<EnqueuerContext>(options =>
                options.UseSqlite("DataSource=:memory:", b => b.MigrationsAssembly("Enqueuer.Telegram.API")));

            var telegramClientDescriptor = services.Single(d => d.ServiceType == typeof(ITelegramBotClient));
            services.Remove(telegramClientDescriptor);
            services.AddHttpClient(nameof(TelegramBotClient))
                .AddTypedClient((_, _) => TelegramClientMock.Object);
        });
    }
}


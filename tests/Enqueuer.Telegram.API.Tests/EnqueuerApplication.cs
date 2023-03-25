using System;
using System.Linq;
using Enqueuer.Data.Configuration;
using Enqueuer.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot;

namespace Enqueuer.Telegram.API.Tests;

public class EnqueuerApplication : WebApplicationFactory<Program>
{
    private const string ConnectionString = "Data Source=:memory:";
    private readonly SqliteConnection _connection;

    public EnqueuerApplication()
    {
        _connection = new SqliteConnection(ConnectionString);
        _connection.Open();
    }

    public Mock<ITelegramBotClient> TelegramClientMock { get; } = new Mock<ITelegramBotClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseTestServer();
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.Single(d => d.ServiceType == typeof(EnqueuerContext));
            services.Remove(dbContextDescriptor);
            services.AddDbContext<EnqueuerContext>(options =>
                options.UseSqlite(_connection, b => b.MigrationsAssembly("Enqueuer.Telegram.API")));

            var telegramClientDescriptor = services.Single(d => d.ServiceType == typeof(ITelegramBotClient));
            services.Remove(telegramClientDescriptor);
            services.AddHttpClient(nameof(TelegramBotClient))
                .AddTypedClient((_, _) => TelegramClientMock.Object);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Close();
    }

    public EnqueuerTelegramClient CreateApplicationClient()
    {
        var client = CreateClient();
        var botConfiguration = Services.GetRequiredService<IBotConfiguration>();
        client.BaseAddress = new UriBuilder("https", "localhost", 8443).Uri;
        return new EnqueuerTelegramClient(client, botConfiguration.AccessToken);
    }
}


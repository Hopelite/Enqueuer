using System;
using Enqueuer.Telegram.Gateway.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Enqueuer.Telegram.Gateway.Startup))]

namespace Enqueuer.Telegram.Gateway;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient(Constants.EnqueuerHttpClient, (services, client) =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(configuration["EnqueuerApiUrl"]);
        });
    }
}

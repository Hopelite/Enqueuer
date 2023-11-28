using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Telegram.Gateway.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Gateway.UpdateProcessing;

public class CallbackProcessor
{
    private readonly HttpClient _apiClient;
    private readonly IConfiguration _configuration;

    public CallbackProcessor(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _apiClient = httpClientFactory.CreateClient(Constants.EnqueuerHttpClient);
        _configuration = configuration;
    }

    [FunctionName(nameof(ProcessCallback))]
    public Task ProcessCallback([ActivityTrigger] Update update, ILogger logger)
    {
        if (update.Type != UpdateType.CallbackQuery)
        {
            throw new ArgumentException($"Callback processor received a non-callback type update with the \"{update.Id}\" ID.");
        }

        return ProcessMessageInternalAsync(update.CallbackQuery!, logger);
    }

    private async Task ProcessMessageInternalAsync(CallbackQuery callbackQuery, ILogger logger)
    {
        if (!CallbackContext.TryCreate(callbackQuery, out var callbackContext))
        {
            return;
        }

        try
        {
            await _apiClient.PostAsync("/callbacks", callbackContext, new JsonMediaTypeFormatter());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception was thrown when processing a callback with the \"{CallbackID}\" ID.", callbackQuery.Id);
        }
    }
}

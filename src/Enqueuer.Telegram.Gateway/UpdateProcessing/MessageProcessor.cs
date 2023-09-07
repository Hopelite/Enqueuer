using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Telegram.Gateway.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Gateway.UpdateProcessing;

public class MessageProcessor
{
    private readonly HttpClient _apiClient;

    public MessageProcessor(IHttpClientFactory httpClientFactory)
    {
        _apiClient = httpClientFactory.CreateClient(Constants.EnqueuerHttpClient);
    }

    [FunctionName(nameof(ProcessMessage))]
    public Task ProcessMessage([ActivityTrigger] Update update, ILogger logger)
    {
        if (update.Type != UpdateType.Message)
        {
            throw new ArgumentException($"Message processor received a non-message type update with the \"{update.Id}\" ID.");
        }

        return ProcessMessageInternalAsync(update.Message!, logger);
    }

    private async Task ProcessMessageInternalAsync(Message message, ILogger logger)
    {
        if (!MessageContext.TryCreate(message, out var messageContext))
        {
            return;
        }

        try
        {
            await _apiClient.PostAsync("/messages", messageContext, new JsonMediaTypeFormatter());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception was thrown when processing a message with the \"{MessageID}\" ID.", message.MessageId);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Enqueuer.Telegram.Gateway.UpdateProcessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.Gateway;

public static class EntryPoint
{
    [FunctionName(nameof(EntryPoint))]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
        [DurableClient] IDurableClient orchestrationClient,
        ILogger logger)
    {
        using var streamReader = new StreamReader(request.Body);
        var json = await streamReader.ReadToEndAsync();

        Update update; // TODO: refactor
        try
        {
            update = JsonConvert.DeserializeObject<Update>(json);
            if (update == null)
            {
                return new BadRequestResult();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            return new BadRequestResult();
        }

        try
        {
            var instanceId = await orchestrationClient.StartNewAsync(nameof(UpdateProcessingOrchestrator), update);
            if (instanceId == null)
            {
                logger.LogError("An error occurred when starting a new orchestration for processing an update with the \"{UpdateID}\" ID.", update.Id);
                return new InternalServerErrorResult();
            }

            return orchestrationClient.CreateCheckStatusResponse(request, instanceId, returnInternalServerErrorOnFailure: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception was thrown when starting a new orchestration for processing an update with the \"{UpdateID}\" ID.", update.Id);
            return new InternalServerErrorResult();
        }
    }
}

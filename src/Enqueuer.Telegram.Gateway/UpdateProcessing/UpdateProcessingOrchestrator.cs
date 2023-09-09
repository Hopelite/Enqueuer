using System.Threading.Tasks;
using Enqueuer.Telegram.Gateway.UpdateProcessing.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Gateway.UpdateProcessing;

public static class UpdateProcessingOrchestrator
{
    [FunctionName(nameof(UpdateProcessingOrchestrator))]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        var telegramUpdate = context.GetInput<Update>();
        if (telegramUpdate == null)
        {
            logger.LogError("Orchestrator received a null update from the entry point function.");
            throw new UpdateProccessingException("An error occurred while proccessing the update.");
        }

        if (telegramUpdate.Type == UpdateType.Message)
        {
            await context.CallActivityAsync(nameof(MessageProcessor.ProcessMessage), telegramUpdate);
        }
        else if (telegramUpdate.Type == UpdateType.CallbackQuery)
        {
            await context.CallActivityAsync(nameof(CallbackProcessor.ProcessCallback), telegramUpdate);
        }
    }
}

using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    public class SwitchQueueDynamicHandler : CallbackHandlerBaseWithReturnToQueueButton
    {
        private readonly IQueueService _queueService;
        private readonly IUserInQueueService _userInQueueService;

        public SwitchQueueDynamicHandler(IQueueService queueService, IUserInQueueService userInQueueService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            _queueService = queueService;
            _userInQueueService = userInQueueService;
        }

        public override string Command => CallbackConstants.SwitchQueueDynamicCommand;

        protected override async Task<Message> HandleCallbackAsyncImplementation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = _queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    var returnButton = GetReturnToChatButton(callbackData);
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: returnButton);
                }

                return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            if (queue.IsDynamic)
            {
                queue.IsDynamic = false;
                await _queueService.UpdateQueueAsync(queue);
                return await botClient.EditMessageTextAsync(
                    callbackQuery.Message.Chat,
                    callbackQuery.Message.MessageId,
                    $"Queue <b>'{queue.Name}' is not dynamic now.</b>",
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callbackData));
            }

            queue.IsDynamic = true;
            await _queueService.UpdateQueueAsync(queue);
            await _userInQueueService.CompressQueuePositionsAsync(queue);

            var chat = queue.Group;
            await botClient.SendTextMessageAsync(
                chat.Id,
                $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName + ' ' ?? string.Empty}made <b>'{queue.Name}'</b> queue dynamic. Keep up!",
                ParseMode.Html);

            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Queue <b>'{queue.Name}' is dynamic now.</b>",
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callbackData));
        }
    }
}

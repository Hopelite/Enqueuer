using Telegram.Bot.Exceptions;

namespace Enqueuer.Data.Exceptions
{
    public class TelegramExceptionsParser : IExceptionParser
    {
        public ApiRequestException Parse(ApiResponse apiResponse)
        {
            if (apiResponse.Description.Contains("Bad Request: message is not modified:"))
            {
                return new MessageNotModifiedException(apiResponse.Description);
            }

            return new(apiResponse.Description, apiResponse.ErrorCode, apiResponse.Parameters);
        }
    }
}

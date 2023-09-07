using Telegram.Bot.Exceptions;

namespace Enqueuer.Messaging.Core.Exceptions;

public class TelegramExceptionsParser : IExceptionParser
{
    public ApiRequestException Parse(ApiResponse apiResponse)
    {
        if (apiResponse.ErrorCode == (int)ErrorCode.NotFound)
        {
            return new NotFoundException(apiResponse.Description, apiResponse.ErrorCode);
        }

        if (/*apiResponse.ErrorCode == (int)ErrorCode.BadRequest && */apiResponse.Description.Contains("Bad Request: message is not modified"))
        {
            return new MessageNotModifiedException(apiResponse.Description, apiResponse.ErrorCode);
        }

        return new(apiResponse.Description, apiResponse.ErrorCode, apiResponse.Parameters);
    }
}

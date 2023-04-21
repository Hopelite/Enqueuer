using Newtonsoft.Json;

namespace Enqueuer.Telegram.Core.Serialization;

public class JsonCallbackDataSerializer : ICallbackDataSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None
    };

    public string Serialize(CallbackData callbackData)
    {
        return JsonConvert.SerializeObject(callbackData, Settings);
    }
}

using Newtonsoft.Json;

namespace Enqueuer.Data.DataSerialization;

/// <summary>
/// Deserializes JSON data.
/// </summary>
public class JsonDataDeserializer : IDataDeserializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Error,
    };

    public T Deserialize<T>(string data)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(data, Settings);
        }
        catch (JsonSerializationException)
        {
            return default;
        }
    }
}

using Newtonsoft.Json;

namespace Enqueuer.Data.DataSerialization;

/// <summary>
/// Serializes data into JSON.
/// </summary>
public class JsonDataSerializer : IDataSerializer
{
    public string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        });
    }
}

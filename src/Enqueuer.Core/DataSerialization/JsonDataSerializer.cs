using Newtonsoft.Json;

namespace Enqueuer.Data.DataSerialization;

/// <summary>
/// Serializes data into JSON.
/// </summary>
public class JsonDataSerializer : IDataSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None
    };

    public string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data, Settings); 
    }
}

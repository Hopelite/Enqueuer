using Newtonsoft.Json;

namespace Enqueuer.Data.DataSerialization
{
    /// <summary>
    /// Deserializes JSON data.
    /// </summary>
    public class JsonDataDeserializer : IDataDeserializer
    {
        /// <inheritdoc/>
        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}

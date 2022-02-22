using Newtonsoft.Json;

namespace Enqueuer.Data.DataSerialization
{
    /// <summary>
    /// Serializes data into JSON.
    /// </summary>
    public class JsonDataSerializer : IDataSerializer
    {
        /// <inheritdoc/>
        public string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}

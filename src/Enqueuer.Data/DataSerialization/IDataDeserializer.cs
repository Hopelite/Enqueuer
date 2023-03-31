namespace Enqueuer.Data.DataSerialization
{
    /// <summary>
    /// Contains deserialization method.
    /// </summary>
    public interface IDataDeserializer
    {
        /// <summary>
        /// Deserializes <paramref name="data"/>.
        /// </summary>
        public T Deserialize<T>(string data);
    }
}

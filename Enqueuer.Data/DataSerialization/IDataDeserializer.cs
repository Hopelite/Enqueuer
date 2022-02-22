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
        /// <typeparam name="T">Type of <paramref name="data"/> to deserialize.</typeparam>
        /// <param name="data">Data to deserialize.</param>
        /// <returns>Deserialized into <typeparamref name="T"/> <paramref name="data"/>.</returns>
        public T Deserialize<T>(string data);
    }
}

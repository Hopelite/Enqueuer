namespace Enqueuer.Data.DataSerialization
{
    /// <summary>
    /// Contains serialization method.
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serializes <paramref name="data"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="data"/> to serialize.</typeparam>
        /// <param name="data">Data to serialize.</param>
        /// <returns>Serialized into string <paramref name="data"/>.</returns>
        public string Serialize<T>(T data);
    }
}

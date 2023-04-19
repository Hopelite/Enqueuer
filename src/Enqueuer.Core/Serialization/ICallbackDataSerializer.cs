namespace Enqueuer.Core.Serialization;

public interface ICallbackDataSerializer
{
    /// <summary>
    /// Serializes the <paramref name="callbackData"/> to <see cref="string"/>.
    /// </summary>
    string Serialize(CallbackData callbackData);
}

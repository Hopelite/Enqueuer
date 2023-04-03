using Enqueuer.Data;
using Enqueuer.Data.Exceptions;
using Newtonsoft.Json;

namespace Enqueuer.Core.Serialization;

public class JsonCallbackDataDeserializer : ICallbackDataDeserializer
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Error,
    };

    /// <inheritdoc/>
    /// <exception cref="OutdatedCallbackException">
    /// Thrown if <paramref name="callbackQueryData"/> contains an invalid schema. Assuming that no other callbacks other than
    /// those created by this application are sent to the application, we treat the invalid scheme as an outdated callback.
    /// </exception>
    public CallbackData? Deserialize(string? callbackQueryData)
    {
        if (string.IsNullOrWhiteSpace(callbackQueryData))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<CallbackData>(callbackQueryData, _settings);
        }
        catch (JsonSerializationException ex)
        {
            throw new OutdatedCallbackException("Received an outdated callback with an invalid schema.", ex);
        }
    }
}

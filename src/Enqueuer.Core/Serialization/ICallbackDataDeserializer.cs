﻿using Enqueuer.Data;

namespace Enqueuer.Core.Serialization;

public interface ICallbackDataDeserializer
{
    /// <summary>
    /// Deserializes the <paramref name="callbackQueryData"/> into <see cref="CallbackData"/>.
    /// </summary>
    CallbackData? Deserialize(string? callbackQueryData);
}

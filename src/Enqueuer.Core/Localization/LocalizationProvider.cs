using System;
using System.Collections.Concurrent;
using System.Globalization;
using Enqueuer.Core.Resources;

namespace Enqueuer.Core.Localization;

public class LocalizationProvider : ILocalizationProvider
{
    private readonly ConcurrentDictionary<FormatMessageKey, string> _formatMessages = new();

    public string GetMessage(string key, MessageParameters messageParameters)
    {
        var formatKey = new FormatMessageKey(key, messageParameters.Culture);
        if (_formatMessages.TryGetValue(formatKey, out var format))
        {
            if (messageParameters.Parameters.Length == 0)
            {
                return format;
            }

            return string.Format(format, messageParameters.Parameters);
        }

        format = Messages.ResourceManager.GetString(key, messageParameters.Culture);
        if (format == null)
        {
            throw new ArgumentException($"There is no message \"{key}\" specified in the \"{messageParameters.Culture.Name}\" resource file.", nameof(key));
        }

        _formatMessages.TryAdd(formatKey, format);
        return string.Format(format, messageParameters.Parameters);
    }

    private sealed record FormatMessageKey(string key, string cultureName)
    {
        public FormatMessageKey(string key, CultureInfo cultureInfo)
            : this(key, cultureInfo.Name)
        {
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Enqueuer.Telegram.Core.Resources;

namespace Enqueuer.Telegram.Core.Localization;

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

    private readonly struct FormatMessageKey : IEquatable<FormatMessageKey>
    {
        private readonly string _key;
        private readonly string _cultureName;

        public FormatMessageKey(string key, CultureInfo cultureInfo)
            : this(key, cultureInfo.Name)
        {
        }

        public FormatMessageKey(string key, string cultureName)
        {
            _key = key;
            _cultureName = cultureName;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is FormatMessageKey messageKey)
            {
                return Equals(messageKey);
            }

            return false;
        }

        public bool Equals(FormatMessageKey messageKey)
        {
            return string.Equals(_key, messageKey._key, StringComparison.InvariantCulture)
                && string.Equals(_cultureName, messageKey._cultureName, StringComparison.CurrentCulture);
        }

        public override int GetHashCode()
        {
            const int firstPrimeNumber = 17;
            const int secondPrimeNumber = 23;

            unchecked
            {
                var hash = firstPrimeNumber;
                hash = hash * secondPrimeNumber + _key.GetHashCode();
                hash = hash * secondPrimeNumber + _cultureName.GetHashCode();
                return hash;
            }
        }
    }
}

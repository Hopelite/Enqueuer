using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Enqueuer.Messaging.Core.Localization;

/// <summary>
/// Contains message parameters for <see cref="ILocalizationProvider"/>.
/// </summary>
public class MessageParameters
{
    /// <summary>
    /// Constant for empty parameters.
    /// </summary>
    public static readonly MessageParameters None = new();

    /// <summary>
    /// Message parameters to be inserted into the message. Can be empty.
    /// </summary>
    public string[] Parameters { get; }

    /// <summary>
    /// Culture used to get the localized message. By default set to the current thread UI culture.
    /// </summary>
    public CultureInfo Culture { get; }

    public MessageParameters()
        : this(Array.Empty<string>())
    {
    }

    public MessageParameters(CultureInfo? cultureInfo)
        : this(cultureInfo, Array.Empty<string>())
    {
    }

    public MessageParameters(params string[] parameters)
        : this(Thread.CurrentThread.CurrentUICulture, parameters)
    {
    }

    public MessageParameters(CultureInfo? cultureInfo, params string[] parameters)
    {
        if (parameters.Any(p => string.IsNullOrWhiteSpace(p)))
        {
            throw new ArgumentException("One of the message parameters is null, empty or a whitespace.");
        }

        Parameters = parameters;
        Culture = cultureInfo ?? Thread.CurrentThread.CurrentUICulture;
    }
}

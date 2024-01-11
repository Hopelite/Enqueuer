using System.Globalization;

namespace Enqueuer.Messaging.Core.Helpers;

public static class ChatConfigurationHelper
{
    public const string DefaultChatCulture = "en-US";

    public static string GetCultureNameFromIetfTag(string? tag)
        => tag == null
            ? DefaultChatCulture
            : CultureInfo.GetCultureInfoByIetfLanguageTag(tag).Name;
}

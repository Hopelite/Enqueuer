namespace Enqueuer.Telegram.Core.Localization;

public interface ILocalizationProvider
{
    /// <summary>
    /// Gets a formatted message by <paramref name="key"/> with the specified <paramref name="messageParameters"/>.
    /// </summary>
    string GetMessage(string key, MessageParameters messageParameters);
}

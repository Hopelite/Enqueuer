namespace Enqueuer.Data.TextProviders;

public interface IMessageProvider
{
    string GetMessage(string key, params object[] args);
}

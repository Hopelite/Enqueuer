namespace Enqueuer.Core.TextProviders;

public interface IMessageProvider
{
    string GetMessage(string key, params object[] args);
}

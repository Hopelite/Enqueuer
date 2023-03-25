using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Enqueuer.Telegram.API.Tests.Utilities;

public class EnqueuerTelegramClient
{
    private readonly HttpClient _httpClient;
    private readonly string _botAccessToken;

    public EnqueuerTelegramClient(HttpClient httpClient, string botAccessToken)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _botAccessToken = string.IsNullOrWhiteSpace(botAccessToken) ? throw new ArgumentNullException(nameof(botAccessToken)) : botAccessToken;
    }

    public async Task PostAsync(Update update)
    {
        var body = JsonConvert.SerializeObject(update);
        var response = await _httpClient.PostAsync($"bot{_botAccessToken}", new StringContent(body, Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new EnqueuerClientException(content);
        }
    }

    internal class EnqueuerClientException : Exception
    {
        public EnqueuerClientException()
        {
        }

        public EnqueuerClientException(string? message)
            : base(message)
        {
        }

        public EnqueuerClientException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

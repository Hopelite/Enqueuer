using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Enqueuer.Telegram.API.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Deserializes the <paramref name="httpContext"/> to <typeparamref name="T"/>.
    /// </summary>
    public static async Task<T> DeserializeBodyAsync<T>(this HttpContext httpContext)
    {
        using var streamReader = new StreamReader(httpContext.Request.Body);
        var json = await streamReader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }
}

using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Enqueuer.Telegram.API.Extensions;

public static class HttpContextExtensions
{
    public static T DeserializeBody<T>(this HttpContext httpContext)
    {
        using var streamReader = new StreamReader(httpContext.Request.Body);
        var json = streamReader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }
}

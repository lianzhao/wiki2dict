using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wiki2Dict.Wiki
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string requestUri,
            ILogger logger)
        {
            if (logger == null)
            {
                return await httpClient.GetAsync(requestUri);
            }

            logger.LogInformation($"Sending request {requestUri}");
            var now = DateTimeOffset.Now;
            var res = await httpClient.GetAsync(requestUri).ConfigureAwait(false);
            if (res.IsSuccessStatusCode)
            {
                logger.LogDebug(
                    $"Got response {res.StatusCode} in {(DateTimeOffset.Now - now).TotalMilliseconds.ToString("F2")}ms");
            }
            else
            {
                logger.LogError(
                    $"Got response {res.StatusCode} in {(DateTimeOffset.Now - now).TotalMilliseconds.ToString("F2")}ms");
            }
            return res;
        }
    }
}
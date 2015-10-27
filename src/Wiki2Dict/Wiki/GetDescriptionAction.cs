using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class GetDescriptionAction : IDictEntryAction
    {
        private readonly ILogger _logger;

        public GetDescriptionAction(ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger(typeof (GetDescriptionAction).FullName);
        }

        public async Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            var tasks = entries.Select(async entry =>
            {
                try
                {
                    var requestUrl =
                        $"api.php?action=query&generator=allpages&gapfrom={entry.Value}&gapto={entry.Value}&exintro&gaplimit=1&prop=extracts&continue=&format=json";
                    var response = await client.GetAsync(requestUrl, _logger).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var model = JsonConvert.DeserializeObject<QueryResponse>(json);
                    var description = model?.query?.pages?.Values.FirstOrDefault()?.extract;
                    if (!string.IsNullOrEmpty(description))
                    {
                        description = $"{description}<br><a href=\"{client.BaseAddress}wiki/{entry.Value}\">读更多...</a>";
                    }
                    entry.Attributes["Description"] = description;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
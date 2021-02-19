using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class GetDescriptionAction : IDictEntryAction
    {
        private const int MAX_REQUEST = 500;

        private readonly ILogger _logger;

        public GetDescriptionAction(ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger(typeof(GetDescriptionAction).FullName);
        }

        public async Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            var tasks = entries.Select((entry, index) => new { entry, index }).GroupBy(pair =>
                    pair.index % MAX_REQUEST
            ).Select(async grp =>
            {
                var groupTasks = grp.Select(async pair =>
                {
                    var entry = pair.entry;
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
                        _logger.LogDebug($"Got description from {entry.Value}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception caught while getting description from {entry.Value}, msg={ex.Message}");
                    }
                });
                await Task.WhenAll(groupTasks).ConfigureAwait(false);
            });
            foreach(var task in tasks){
                await task.ConfigureAwait(false);
            }
            //await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
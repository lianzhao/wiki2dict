using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class GetDescriptionAction : IDictEntryAction
    {
        public async Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            var tasks = entries.Select(async entry =>
            {
                var requestUrl =
                    $"api.php?action=query&generator=allpages&gapfrom={entry.Value}&gapto={entry.Value}&exintro&gaplimit=1&prop=extracts&continue=&format=json";
                var response = await client.GetAsync(requestUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var model = JsonConvert.DeserializeObject<QueryResponse>(json);
                var description = model.query.pages.Values.FirstOrDefault()?.extract;
                entry.Attributes["Description"] = description;
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
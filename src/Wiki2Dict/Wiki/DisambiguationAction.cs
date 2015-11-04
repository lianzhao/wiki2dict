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
    public class DisambiguationAction : IDictEntryAction
    {
        private readonly ILogger _logger;

        public DisambiguationAction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof (DisambiguationAction).FullName);
        }

        public async Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            // Step 1
            // Get disambiguation pages
            var disambiguationPages = await GetPagesInCategory(client, "消歧义页").ConfigureAwait(false);

            // Step 2
            // Get pages that may ambiguous
            var tasks =
                disambiguationPages.Select(
                    async p =>
                        new
                        {
                            title = p.title,
                            pages = await GetPagesInCategory(client, $"{p.title}的歧义页面").ConfigureAwait(false)
                        });
            var groups = await Task.WhenAll(tasks).ConfigureAwait(false);

            //Step 3
            //Update entries
            foreach (var @group in groups.Where(g => g.pages != null))
            {
                //var title = group.title.Replace("(消歧义)", string.Empty);
                var alterKeys = group.pages.Select(p => TrimTitle(p.title)).ToList();
                foreach (var entry in @group.pages.Select(page => entries.FirstOrDefault(e => e.Value == page.title)))
                {
                    //entry.Value = title;
                    entry?.AlternativeKeys.AddRange(alterKeys);
                }
            }
        }

        private async Task<IEnumerable<Page>> GetPagesInCategory(HttpClient client, string category)
        {
            var res =
                await client.GetAsync(
                    $"/api.php?action=query&generator=categorymembers&gcmtitle=Category:{category}&gcmlimit=max&format=json&continue=",
                    _logger)
                    .ConfigureAwait(false);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            var model = JsonConvert.DeserializeObject<QueryResponse>(json);
            return model?.query?.pages?.Values;
        }

        private static string TrimTitle(string title)
        {
            var index = title.IndexOf("(", StringComparison.Ordinal);
            return index > 0 ? title.Substring(0, index) : title;
        }
    }
}
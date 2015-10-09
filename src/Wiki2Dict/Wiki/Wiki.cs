using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class Wiki : IWiki
    {
        private readonly HttpClient _httpClient;

        private readonly IDictEntryAction _furtherAction;

        public Wiki(HttpClient httpClient, IDictEntryAction furtherAction = null)
        {
            _httpClient = httpClient;
            _furtherAction = furtherAction;
        }

        public async Task<WikiDescription> GetDescriptionAsync()
        {
            var requestUrl = "api.php?action=query&meta=siteinfo&format=json";
            var response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var siteInfo = JsonConvert.DeserializeObject<SiteInfoQueryResponse>(json);
            return new WikiDescription() { Name = siteInfo.query.general.sitename, Url = _httpClient.BaseAddress.ToString(), CopyrightUrl = $"{_httpClient.BaseAddress.ToString()}wiki/Project:Copyright" };
        }

        public async Task<IEnumerable<DictEntry>> GetEntriesAsync()
        {
            var redirectsQuery = await GetAllRedirectsAsync().ConfigureAwait(false);
            var redirects =
                redirectsQuery.Select(r =>
                {
                    if (r.links == null)
                    {
                        Console.WriteLine($"Warning: redirect page {r.title} does not contains links.");
                    }
                    return new { RedirectFrom = r.title, RedirectTo = r.links?.FirstOrDefault()?.title };
                }
                    )
                    .Where(r => !string.IsNullOrEmpty(r.RedirectTo))
                    .GroupBy(r => r.RedirectTo);
            var langlinksQuery = await GetAllLanglinksAsync().ConfigureAwait(false);
            var pages =
                langlinksQuery.Select(p =>
                {
                    if (p.langlinks == null)
                    {
                        Console.WriteLine($"Warning: page {p.title} dose not contains en link.");
                    }
                    return new { Title = p.title, Lang = p.langlinks?.FirstOrDefault()?._ };
                })
                    .Where(p => !string.IsNullOrEmpty(p.Lang));

            var entries = pages.Join(redirects, page => page.Title, redirect => redirect.Key,
                    (page, redirect) => new DictEntry
                    {
                        Key = page.Lang,
                        AlternativeKeys =
                            redirect.Where(r => r.RedirectFrom != page.Lang).Select(r => r.RedirectFrom).ToList(),
                        Value = page.Title,
                    }).ToList();

            if (_furtherAction != null)
            {
                await _furtherAction.InvokeAsync(_httpClient, entries).ConfigureAwait(false);
            }

            return entries.OrderBy(e => e.Key);
        }

        private async Task<IEnumerable<Page>> GetAllLanglinksAsync()
        {
            return await GetAllPagesAsync(async gapcontinue => await GetLanglinksAsync(gapcontinue).ConfigureAwait(false));
        }

        private async Task<IEnumerable<Page>> GetAllRedirectsAsync()
        {
            return await GetAllPagesAsync(async gapcontinue => await GetRedirectsAsync(gapcontinue).ConfigureAwait(false));
        }

        private static async Task<IEnumerable<Page>> GetAllPagesAsync(Func<string, Task<QueryResponse>> getPagesFunc)
        {
            var rv = Enumerable.Empty<Page>();
            string gapcontinue = null;
            while (true)
            {
                var response = await getPagesFunc(gapcontinue).ConfigureAwait(false);
                gapcontinue = response._continue?.gapcontinue;
                rv = rv.Concat(response.query.pages.Values);
                if (string.IsNullOrEmpty(gapcontinue))
                {
                    break;
                }
            }
            return rv;
        }

        private async Task<QueryResponse> GetLanglinksAsync(string gapcontinue)
        {
            var requestUrl =
                "api.php?action=query&generator=allpages&gapnamespace=0&gaplimit=max&lllimit=max&gapfilterredir=nonredirects&gapfilterlanglinks=withlanglinks&lllang=en&prop=langlinks&format=json&continue=";
            return await GetPagesAsync(requestUrl, gapcontinue).ConfigureAwait(false);
        }

        private async Task<QueryResponse> GetRedirectsAsync(string gapcontinue)
        {
            var requestUrl =
                "api.php?action=query&generator=allpages&gapnamespace=0&gaplimit=max&pllimit=max&gapfilterredir=redirects&prop=links&format=json&continue=";
            return await GetPagesAsync(requestUrl, gapcontinue).ConfigureAwait(false);
        }

        private async Task<QueryResponse> GetPagesAsync(string requestUrl, string gapcontinue)
        {
            Console.WriteLine($"Entering GetPagesAsync");
            var enterTime = DateTimeOffset.Now;

            if (!string.IsNullOrEmpty(gapcontinue))
            {
                requestUrl = $"{requestUrl}gapcontinue||&gapcontinue={gapcontinue}";
            }
            Console.WriteLine($"{nameof(requestUrl)}={requestUrl}");

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rv = JsonConvert.DeserializeObject<QueryResponse>(json);
            Console.WriteLine($"Exiting GetPagesAsync, {(DateTimeOffset.Now - enterTime).TotalSeconds.ToString("F2")} s");
            return rv;
        }
    }
}

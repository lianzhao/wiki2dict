using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToWiki;
using LinqToWiki.Generated;
using Wiki2Dict.Core;

namespace Wiki2Dict.LinqToWiki
{
    public class LinqToWiki : IWiki
    {
        private readonly IDictEntryAction _furtherAction;

        protected readonly Wiki _wiki;

        public LinqToWiki(string baseAddr, IDictEntryAction furtherAction = null)
            : this(new Wiki("Wiki2Dict", baseAddr), furtherAction)
        {
        }

        public LinqToWiki(Wiki wiki, IDictEntryAction furtherAction = null)
        {
            _wiki = wiki;
            _furtherAction = furtherAction;
        }

        public Task<IEnumerable<DictEntry>> GetEntriesAsync()
        {
            return Task.Run(async () =>
            {
                var redirectsQuery =
                    _wiki.Query.allpages()
                        .Where(p => p.filterredir == allpagesfilterredir.redirects && p.ns == Namespace.Article)
                        .Pages.Select(p => PageResult.Create(p.info, p.links().ToEnumerable()))
                        .ToEnumerable();
                var redirects =
                    redirectsQuery.Select(
                        redirect => new { redirect.Info, RedirectTo = redirect.Data.FirstOrDefault()?.title })
                        .Where(redirect => !string.IsNullOrEmpty(redirect.RedirectTo))
                        .GroupBy(redirect => redirect.RedirectTo);

                var pagesQuery =
                    _wiki.Query.allpages()
                        .Where(
                            p =>
                                p.filterredir == allpagesfilterredir.nonredirects && p.ns == Namespace.Article &&
                                p.filterlanglinks == allpagesfilterlanglinks.withlanglinks)
                        .Pages.Select(
                            p => PageResult.Create(p.info, p.langlinks().Where(l => l.lang == "en").ToEnumerable()))
                        .ToEnumerable();
                var pages =
                    pagesQuery.Select(page => new { page.Info, Lang = page.Data.FirstOrDefault()?.value })
                        .Where(p => !string.IsNullOrEmpty(p.Lang));

                var entries = pages.Join(redirects, page => page.Info.title, redirect => redirect.Key,
                    (page, redirect) => new DictEntry
                    {
                        Key = page.Lang,
                        AlternativeKeys =
                            redirect.Where(r => r.Info.title != page.Info.title).Select(r => r.Info.title).ToList(),
                        Value = page.Info.title,
                    }).ToList();

                if (_furtherAction != null)
                {
                    await _furtherAction.InvokeAsync(_wiki, entries).ConfigureAwait(false);
                }

                return entries.AsEnumerable();
            });
        }
    }
}
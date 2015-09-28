using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToWiki.Generated;
using Wiki2Dict.Core;

namespace Wiki2Dict.LinqToWiki
{
    public interface IDictEntryAction
    {
        Task InvokeAsync(Wiki wiki, IList<DictEntry> entries);
    }
}
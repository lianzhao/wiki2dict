using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public interface IDictEntryAction
    {
        Task InvokeAsync(HttpClient client, IList<DictEntry> entries);
    }
}
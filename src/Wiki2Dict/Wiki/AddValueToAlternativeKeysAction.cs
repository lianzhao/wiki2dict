using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class AddValueToAlternativeKeysAction : IDictEntryAction
    {
        public Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.AlternativeKeys.Contains(entry.Value))
                {
                    continue;
                }
                entry.AlternativeKeys.Add(entry.Value);
            }
            return Task.FromResult(0);
        }
    }
}
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
                var value = entry.Value.EscapeForXml();
                TryAddAlternativeKey(entry, value);
                TryAddAlternativeKey(entry, value.Replace("•", "·"));
                TryAddAlternativeKey(entry, value.Replace("·", "•"));
            }
            return Task.FromResult(0);
        }
        
        private bool TryAddAlternativeKey(DictEntry entry, string alternativeKey){
            if (entry.AlternativeKeys.Contains(alternativeKey)){
                return false;
            }
            entry.AlternativeKeys.Add(alternativeKey);
            return true;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class CompositeDictEntryAction : IDictEntryAction
    {
        private readonly IEnumerable<IDictEntryAction> actions;

        public CompositeDictEntryAction(params IDictEntryAction[] actions) : this((IEnumerable<IDictEntryAction>) actions.AsEnumerable())
        {
        }

        public CompositeDictEntryAction(IEnumerable<IDictEntryAction> actions)
        {
            this.actions = actions;
        }

        public async Task InvokeAsync(HttpClient client, IList<DictEntry> entries)
        {
            if (actions == null)
            {
                return;
            }

            var tasks = actions.Select(action => action.InvokeAsync(client, entries));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
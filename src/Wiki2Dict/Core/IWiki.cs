using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wiki2Dict.Core
{
    public interface IWiki
    {
        Task<IEnumerable<DictEntry>> GetEntriesAsync();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wiki2Dict.Core
{
    public interface IWiki
    {
        Task<WikiDescription> GetDescriptionAsync();

        Task<IEnumerable<DictEntry>> GetEntriesAsync();
    }
}
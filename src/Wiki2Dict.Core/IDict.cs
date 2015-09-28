using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wiki2Dict.Core
{
    public interface IDict
    {
        Task SaveAsync(IEnumerable<DictEntry> entries);
    }
}
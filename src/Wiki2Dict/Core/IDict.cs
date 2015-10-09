using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wiki2Dict.Core
{
    public interface IDict
    {
        Task SaveAsync(WikiDescription wiki, IEnumerable<DictEntry> entries);
    }
}
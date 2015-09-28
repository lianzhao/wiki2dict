using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Kindle
{
    public class Dict : IDict
    {
        public Task SaveAsync(IEnumerable<DictEntry> entries)
        {
            throw new NotImplementedException();
        }
    }
}

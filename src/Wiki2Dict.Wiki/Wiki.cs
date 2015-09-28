using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Wiki
{
    public class Wiki : IWiki
    {
        public Task<IEnumerable<DictEntry>> GetEntriesAsync()
        {
            throw new NotImplementedException();
        }
    }
}

using System.Collections.Generic;

namespace Wiki2Dict.Core
{
    public class DictEntry
    {
        public string Key { get; set; }

        public IEnumerable<string> AlternativeKeys { get; set; }

        public string Value { get; set; }

        public IDictionary<string, string> Attributes { get; set; }
    }
}

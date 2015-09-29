using System.Collections.Generic;

namespace Wiki2Dict.Core
{
    public class DictEntry
    {
        public DictEntry()
        {
            Attributes = new Dictionary<string, string>();
        }

        public string Key { get; set; }

        public IEnumerable<string> AlternativeKeys { get; set; }

        public string Value { get; set; }

        public IDictionary<string, string> Attributes { get; set; }
    }
}

using Newtonsoft.Json;

namespace Wiki2Dict.Wiki
{
    public class Continue
    {
        public string plcontinue { get; set; }
        public string gapcontinue { get; set; }
        [JsonProperty("continue")]
        public string _continue { get; set; }
    }
}
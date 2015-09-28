using Newtonsoft.Json;

namespace Wiki2Dict.Wiki
{
    public class Langlink
    {
        public string lang { get; set; }
        [JsonProperty("*")]
        public string _ { get; set; }
    }
}
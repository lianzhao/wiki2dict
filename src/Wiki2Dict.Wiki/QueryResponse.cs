using Newtonsoft.Json;

namespace Wiki2Dict.Wiki
{
    public class QueryResponse
    {
        [JsonProperty("continue")]
        public Continue _continue { get; set; }
        public Query query { get; set; }
    }
}
namespace Wiki2Dict.Wiki
{
    public class Page
    {
        public int pageid { get; set; }
        public int ns { get; set; }
        public string title { get; set; }
        public string extract { get; set; }
        public Link[] links { get; set; }
        public Langlink[] langlinks { get; set; }
    }
}
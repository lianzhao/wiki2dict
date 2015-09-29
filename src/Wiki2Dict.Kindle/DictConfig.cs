namespace Wiki2Dict.Kindle
{
    public class DictConfig
    {
        public DictConfig()
        {
            iformFormat = "<idx:iform name=\"\" value=\"{0}\" />";
        }

        public string FilePath { get; set; }

        public string TemplateFilePath { get; set; }

        public string EntryTemplateFilePath { get; set; }

        public string iformFormat { get; set; }
    }
}
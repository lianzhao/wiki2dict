namespace Wiki2Dict
{
    public static class StringExtensions
    {
        public static string EscapeForXml(this string input)
        {
            return input?.Replace("\"", "&quot;");
        }
    }
}
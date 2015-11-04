using System;

namespace Wiki2Dict
{
    public static class StringExtensions
    {
        public static string EscapeForXml(this string input)
        {
            return input?.Replace("\"", "&quot;");
        }

        public static string TrimWikiPageTitle(this string title)
        {
            var index = title.IndexOf("(", StringComparison.Ordinal);
            return index > 0 ? title.Substring(0, index) : title;
        }
    }
}
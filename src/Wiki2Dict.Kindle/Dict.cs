using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Kindle
{
    public class Entry
    {
        public string orth { get; set; }

        public string infl { get; set; }

        public string word { get; set; }

        public string phonetic { get; set; }

        public string cat { get; set; }

        public string description { get; set; }
    }

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

    public class Dict : IDict
    {
        private readonly DictConfig _config;

        public Dict(DictConfig config)
        {
            _config = config;
        }

        public async Task SaveAsync(IEnumerable<DictEntry> entries)
        {
            string dictTemplate;
            using (var sr = new StreamReader(new FileStream(_config.TemplateFilePath, FileMode.Open)))
            {
                dictTemplate = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(dictTemplate))
            {
                throw new InvalidOperationException("Dict template file error.");
            }

            string entryTemplate;
            using (var sr = new StreamReader(new FileStream(_config.EntryTemplateFilePath, FileMode.Open)))
            {
                entryTemplate = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(entryTemplate))
            {
                throw new InvalidOperationException("Dict entry template file error.");
            }

            var entriesXml = string.Join(string.Empty,
                entries.Select(entry => FormatEntry(entryTemplate, ConvertEntry(entry))));
            var xml = dictTemplate.Replace("@entries", entriesXml);
            using (var sw = new StreamWriter(new FileStream(_config.FilePath, FileMode.OpenOrCreate)))
            {
                await sw.WriteAsync(xml).ConfigureAwait(false);
            }
        }

        private Entry ConvertEntry(DictEntry dictEntry)
        {
            var rv = new Entry
            {
                orth = dictEntry.Key,
                infl =
                    string.Join(string.Empty,
                        dictEntry.AlternativeKeys.Select(key => string.Format(_config.iformFormat, key))),
                word = dictEntry.Key,
                phonetic = dictEntry.Attributes.ContainsKey("Phonetic") ? dictEntry.Attributes["Phonetic"] : null,
                description =
                    dictEntry.Attributes.ContainsKey("Description") ? dictEntry.Attributes["Description"] : null,
            };
            return rv;
        }

        private static string FormatEntry(string template, Entry entry)
        {
            var rv = template;
            var properties = typeof (Entry).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetGetMethod().Invoke(entry, new object[] {}) as string;
                rv = rv.Replace(string.Format("@{0}", property.Name), value);
            }

            return rv;
        }
    }
}

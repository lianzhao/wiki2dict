using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wiki2Dict.Core;

namespace Wiki2Dict.Kindle
{
    public class Dict : IDict
    {
        private readonly DictConfig _config;

        public Dict(DictConfig config)
        {
            _config = config;
        }

        public async Task SaveAsync(WikiDescription wiki, IEnumerable<DictEntry> entries)
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

            string opfTemplate;
            using (var sr = new StreamReader(new FileStream(_config.OpfTemplateFilePath, FileMode.Open)))
            {
                opfTemplate = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(opfTemplate))
            {
                throw new InvalidOperationException("Opf template file error.");
            }
            
            File.Delete(_config.FilePath);
            var entriesXml = string.Join(string.Empty,
                entries.Select(entry => FormatEntry(entryTemplate, ConvertEntry(entry))));
            var xml = dictTemplate.Replace("@entries", entriesXml).Replace("@wikiName", wiki.Name).Replace("@wikiDescription", wiki.Description).Replace("@wikiCopyrightUrl", wiki.CopyrightUrl);
            using (var sw = new StreamWriter(new FileStream(_config.FilePath, FileMode.Create)))
            {
                await sw.WriteAsync(xml).ConfigureAwait(false);
            }

            var opf = opfTemplate.Replace("@wikiName", wiki.Name)
                .Replace("@date", DateTime.Today.ToString("yyyy-MM-dd"));
            var opfFilePath = Path.Combine(_config.OpfFilePath, $"{wiki.Name}_dict.opf");
            File.Delete(opfFilePath);
            using (var sw = new StreamWriter(new FileStream(opfFilePath, FileMode.Create)))
            {
                await sw.WriteAsync(opf).ConfigureAwait(false);
            }
        }

        private Entry ConvertEntry(DictEntry dictEntry)
        {
            var rv = new Entry
            {
                orth = dictEntry.Key,
                infl =
                    string.Join(string.Empty,
                        dictEntry.AlternativeKeys.Select(key => string.Format(_config.iformFormat, key.EscapeForXml()))),
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
            var properties = typeof(Entry).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetGetMethod().Invoke(entry, new object[] { }) as string;
                rv = rv.Replace($"@{property.Name}", value);
            }

            return rv;
        }
    }
}

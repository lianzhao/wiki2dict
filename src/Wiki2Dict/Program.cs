using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Configuration;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Configuration.Json;
using Microsoft.Framework.Logging;
using Wiki2Dict.Core;
using Wiki2Dict.Kindle;
using Wiki2Dict.Wiki;

namespace Wiki2Dict
{
    class Program
    {
        private const string DictEntryActionKey = "DictEntryActionKey";

        static void Main(string[] args)
        {
            var enterTime = DateTimeOffset.Now;
            Console.WriteLine("Start");
            Run(args).Wait();
            Console.WriteLine($"End, {(DateTimeOffset.Now - enterTime).TotalSeconds.ToString("F2")} s");
        }

        private static async Task Run(string[] args)
        {
            try
            {
                // This wont work...
                // See https://github.com/aspnet/Configuration/issues/214
                //var config = new ConfigurationBuilder();
                //config.AddJsonFile("wiki.json");

                var jsonConfig = new JsonConfigurationSource("wiki.json");
                var config = new ConfigurationBuilder(jsonConfig);

                var builder = new ContainerBuilder();
                var module = new ConfigurationModule(config.Build());
                builder.RegisterModule(module);
                
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddConsole(LogLevel.Information);
                builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();

                builder.RegisterType<GetDescriptionAction>().AsImplementedInterfaces().InstancePerDependency();
                builder.RegisterType<AddValueToAlternativeKeysAction>().AsImplementedInterfaces().InstancePerDependency();
                builder.Register(ctx => new CompositeDictEntryAction(ctx.Resolve<IEnumerable<IDictEntryAction>>()))
                    .Keyed<IDictEntryAction>(DictEntryActionKey);
                builder.Register(
                    ctx =>
                        new Wiki.Wiki(ctx.Resolve<HttpClient>(),
                            ctx.ResolveOptionalKeyed<IDictEntryAction>(DictEntryActionKey),
                            ctx.Resolve<ILoggerFactory>()))
                    .AsImplementedInterfaces()
                    .InstancePerDependency();
                builder.RegisterInstance(new DictConfig
                {
                    FilePath = Path.Combine("..", "..", "resources", "kindle_dict.html"),
                    OpfFilePath = Path.Combine("..", "..", "resources"),
                    TemplateFilePath = Path.Combine("..", "..", "resources", "knidle_dict_template.html"),
                    EntryTemplateFilePath = Path.Combine("..", "..", "resources", "knidle_dict_entry_template.html"),
                    OpfTemplateFilePath = Path.Combine("..", "..", "resources", "kindle_dict_template.opf"),
                }).SingleInstance();
                builder.RegisterType<Dict>().AsImplementedInterfaces().SingleInstance();

                var container = builder.Build();

                var wiki = container.Resolve<IWiki>();
                var wikiDesc = await wiki.GetDescriptionAsync().ConfigureAwait(false);
                var entries = await wiki.GetEntriesAsync().ConfigureAwait(false);
                var dict = container.Resolve<IDict>();
                await dict.SaveAsync(wikiDesc, entries).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
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
                var config = new ConfigurationBuilder();
                config.AddJsonFile("wiki.json");

                var builder = new ContainerBuilder();
                var module = new ConfigurationModule(config.Build());
                builder.RegisterModule(module);
                
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
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
                    FilePath = Path.Combine("resources", "kindle_dict.html"),
                    OpfFilePath = Path.Combine("resources"),
                    TemplateFilePath = Path.Combine("resources", "knidle_dict_template.html"),
                    EntryTemplateFilePath = Path.Combine("resources", "knidle_dict_entry_template.html"),
                    OpfTemplateFilePath = Path.Combine("resources", "kindle_dict_template.opf"),
                }).SingleInstance();
                builder.RegisterType<Dict>().AsImplementedInterfaces().SingleInstance();

                var logger = loggerFactory.CreateLogger(typeof (Program).FullName);
                var container = builder.Build();

                var wiki = container.Resolve<IWiki>();
                logger.LogInformation("Getting wiki description...");
                var wikiDesc = await wiki.GetDescriptionAsync().ConfigureAwait(false);
                logger.LogInformation("Getting entries...");
                var entries = await wiki.GetEntriesAsync().ConfigureAwait(false);
                var dict = container.Resolve<IDict>();
                logger.LogInformation("Saving dictionary...");
                await dict.SaveAsync(wikiDesc, entries).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

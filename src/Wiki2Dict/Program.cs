using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Wiki2Dict.Core;

namespace Wiki2Dict
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args).Wait();
        }

        private static async Task Run(string[] args)
        {
            try
            {
                var baseAddr = "http://coppermind.huiji.wiki/";
                var builder = new ContainerBuilder();
                builder.Register(ctx => new HttpClient() {BaseAddress = new Uri(baseAddr)}).InstancePerDependency();
                builder.Register(ctx => new Wiki.Wiki(ctx.Resolve<HttpClient>()))
                    .AsImplementedInterfaces()
                    .InstancePerDependency();

                var container = builder.Build();

                var wiki = container.Resolve<IWiki>();
                var entries = await wiki.GetEntriesAsync().ConfigureAwait(false);
                var dict = container.Resolve<IDict>();
                await dict.SaveAsync(entries).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

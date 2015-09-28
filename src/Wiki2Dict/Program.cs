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
            var builder = new ContainerBuilder();
            

            var container = builder.Build();

            var wiki = container.Resolve<IWiki>();
            var entries = await wiki.GetEntriesAsync().ConfigureAwait(false);
            var dict = container.Resolve<IDict>();
            await dict.SaveAsync(entries).ConfigureAwait(false);
        }
    }
}

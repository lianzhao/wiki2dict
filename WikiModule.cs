using System;
using System.Net.Http;
using Autofac;

namespace Wiki2Dict
{
    public class WikiModule : Module
    {
        private readonly string _baseAddress;

        public WikiModule(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(ctx => new HttpClient() {BaseAddress = new Uri(_baseAddress)}).InstancePerDependency();
        }
    }
}
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Eshopworld.Web
{
    /// <summary>
    /// extension entrypoints for <see cref="IServiceCollection"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// adds AutoRest client to the service collection infusing it with <see cref="HttpClient"/> obtained from <see cref="IHttpClientFactory"/>
        /// </summary>
        /// <typeparam name="TInt">interface matching the API spec</typeparam>
        /// <typeparam name="TImpl">generated AutoRest client type</typeparam>
        /// <param name="services">extension entry point</param>
        /// <returns><see cref="IHttpClientBuilder"/> so that http client can be further configured</returns>
        public static IHttpClientBuilder AddAutoRestClient<TInt, TImpl>(this IServiceCollection services)
            where TInt : class
            where TImpl : class, TInt
        {
            var name = TypeNameHelper.GetTypeDisplayName(typeof(TInt));
            var builder = new EswDefaultHttpClientBuilder(services, name);

            builder.AddTransient<TInt, TImpl>();

            return builder;
        }

        private class EswDefaultHttpClientBuilder : IHttpClientBuilder
        {
            public EswDefaultHttpClientBuilder(IServiceCollection services, string name)
            {
                Services = services;
                Name = name;
            }

            public string Name { get; }

            public IServiceCollection Services { get; }

            internal void AddTransient<TInt, TImpl>()
                where TInt : class
                where TImpl : class, TInt
            {
                Services.AddHttpClient();

                Services.AddTransient(provider =>
                {
                    var factory = provider.GetRequiredService<IHttpClientFactory>();
                    var client = factory.CreateClient(Name);
                    return (TInt)Activator.CreateInstance(typeof(TImpl), client, false);
                });
            }
        }
    }
}

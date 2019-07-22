using System;
using System.Net.Http;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class AutoRestExtensionsTests
    {
        [Fact, IsLayer0]
        public void ResolveAutoRestClientTest()
        {
            var builder = new WebHostBuilder().UseStartup<TestStartup>();
            var host = builder.Build();

            //resolve
            var client = host.Services.GetService<ISwaggerPetstore>();
            client.Should().NotBeNull();
            client.Should().BeOfType<SwaggerPetstore>();
        }

        private class TestStartup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services
                    .AddAutoRestTypedClient<ISwaggerPetstore, SwaggerPetstore>();

                return services.BuildServiceProvider();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
            }
        }

        /// <summary>
        /// mimic AutoRest client interface, dummy for tests
        /// </summary>
        private interface ISwaggerPetstore
        {

        }

        /// <summary>
        /// mimic AutoRest generated client, constructor is the critical piece here
        /// </summary>
        private class SwaggerPetstore : ServiceClient<SwaggerPetstore>, ISwaggerPetstore
        {
            public SwaggerPetstore(HttpClient httpClient, bool disposeHttpClient) : base(httpClient, disposeHttpClient)
            {
            }
        }
    }
}

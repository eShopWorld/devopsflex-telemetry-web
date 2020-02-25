using System;
using System.Net;
using System.Threading.Tasks;

using Eshopworld.Core;
using Eshopworld.Telemetry;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Eshopworld.Web.Tests
{
    public class AlwaysThrowsTestStartupWithServiceUnavailableErrorStatusCode
    {
        internal static readonly BigBrother Bb = new BigBrother("", "");

        public AlwaysThrowsTestStartupWithServiceUnavailableErrorStatusCode(IWebHostEnvironment env)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBigBrother>(Bb);
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBigBrotherExceptionHandler(HttpStatusCode.ServiceUnavailable);

            app.Run(async ctx =>
                        {
                            await Task.Yield();
                            throw new Exception("KABUM!!!");
                        });
        }
    }
}
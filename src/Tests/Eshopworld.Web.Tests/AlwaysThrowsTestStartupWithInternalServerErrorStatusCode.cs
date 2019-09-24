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
    public class AlwaysThrowsTestStartupWithInternalServerErrorStatusCode
    {
        internal static readonly BigBrother Bb = new BigBrother("", "");

        public AlwaysThrowsTestStartupWithInternalServerErrorStatusCode(IHostingEnvironment env)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBigBrother>(Bb);
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBigBrotherExceptionHandler(HttpStatusCode.InternalServerError);

            app.Run(async ctx =>
                        {
                            await Task.Yield();
                            throw new Exception("KABUM!!!");
                        });
        }
    }
}
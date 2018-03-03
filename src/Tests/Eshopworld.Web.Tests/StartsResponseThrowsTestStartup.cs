namespace Eshopworld.Web.Tests
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using DevOpsFlex.Telemetry;

    public class StartsResponseThrowsTestStartup
    {
        internal static readonly BigBrother Bb = new BigBrother("", "");

        public StartsResponseThrowsTestStartup(IHostingEnvironment env)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBigBrother>(Bb);
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBigBrotherExceptionHandler();

            app.Run(async ctx =>
            {
                ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                ctx.Response.ContentType = "text/xml";
                ctx.Response.ContentLength = 200;
                await ctx.Response.Body.FlushAsync();
                throw new ApplicationException();
            });
        }
    }
}

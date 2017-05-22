namespace DevOpsFlex.Telemetry.Web.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class TestStartup
    {
        internal static readonly BigBrother Bb = new BigBrother("", "");

        public TestStartup(IHostingEnvironment env)
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
                await Task.Yield();
                throw new Exception("KABUM!!!");
            });
        }
    }
}

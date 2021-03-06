﻿using System;
using System.Net;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Eshopworld.Web.Tests
{
    public class StartsResponseThrowsTestStartup
    {
        internal static readonly BigBrother Bb = new BigBrother("", "");

        public StartsResponseThrowsTestStartup(IWebHostEnvironment env)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBigBrother>(Bb);
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

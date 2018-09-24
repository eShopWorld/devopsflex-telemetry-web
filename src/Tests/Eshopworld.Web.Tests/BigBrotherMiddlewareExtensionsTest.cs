namespace Eshopworld.Web.Tests
{
    using System;
    using System.Threading.Tasks;
    using DevOpsFlex.Tests.Core;
    using Core;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    public class BigBrotherMiddlewareExtensionsTest
    {
        [Fact, IsIntegration]
        public async Task Test_Middleware_PushesToBigBrother()
        {
            var blewUp = false;
            var server = new TestServer(
                new WebHostBuilder().UseStartup<AlwaysThrowsTestStartup>());

            var client = server.CreateClient();
            var (stream, _, _) = AlwaysThrowsTestStartup.Bb;

            using (stream.Subscribe(
                e =>
                {
                    blewUp = true;
                    e.Should().BeOfType<ExceptionEvent>();
                }))
            {
                await client.GetAsync("/");
                blewUp.Should().BeTrue();
            }
        }

        [Fact, IsIntegration]
        public async Task Test_MiddleWare_PushesToBigBrotherWhenResponseStarted()
        {
            var blewUp = false;
            var server = new TestServer(
                new WebHostBuilder().UseStartup<StartsResponseThrowsTestStartup>());

            var client = server.CreateClient();
            var (stream, _, _) = StartsResponseThrowsTestStartup.Bb;

            using (stream.Subscribe(
                e =>
                {
                    blewUp = true;
                    e.Should().BeOfType<ResponseAlreadyStartedExceptionEvent>();                    
                }))
            {
                await client.GetAsync("/");
                blewUp.Should().BeTrue();
            }
        }
    }
}

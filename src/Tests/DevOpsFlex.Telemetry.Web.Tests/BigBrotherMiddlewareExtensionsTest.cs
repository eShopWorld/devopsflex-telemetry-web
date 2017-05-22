namespace DevOpsFlex.Telemetry.Web.Tests
{
    using System;
    using System.Threading.Tasks;
    using Core;
    using DevOpsFlex.Tests.Core;
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
                new WebHostBuilder().UseStartup<TestStartup>());

            var client = server.CreateClient();
            var (stream, _, _) = TestStartup.Bb;

            using (stream.Subscribe(
                e =>
                {
                    blewUp = true;
                    e.Should().BeOfType<BbExceptionEvent>();
                }))
            {
                await client.GetAsync("/");
                blewUp.Should().BeTrue();
            }
        }
    }
}

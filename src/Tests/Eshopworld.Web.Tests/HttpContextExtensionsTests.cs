using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class HttpContextExtensionsTests
    {
        [Fact, IsLayer0]
        public async Task PerformSecurityChecks_WhenNoPolicyEvaluator_ThrowsInvalidOperationException()
        {
            var ctx2 = CreateTestHttpContextWithNoServices();

            Func<Task> securityCheckAction = async () => await HttpContextExtensions.PerformSecurityChecks(ctx2, string.Empty);
            securityCheckAction.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Be("Unable to obtain Microsoft.AspNetCore.Authorization.Policy.IPolicyEvaluator from the container");
        }

        
        private static HttpContext CreateTestHttpContextWithNoServices()
        {
            var ctx = new DefaultHttpContext();
            var sc = new ServiceCollection();

            ctx.RequestServices = sc.BuildServiceProvider();

            return ctx;
        }
    }
}

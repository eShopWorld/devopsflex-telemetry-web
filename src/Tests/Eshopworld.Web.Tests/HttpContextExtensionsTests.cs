using System;
using System.Security.Claims;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class HttpContextExtensionsTests
    {
        [Fact, IsLayer0]
        public void When_IPolicyEvaluatorNotRegistered_ShouldThrowInvalidOperationException()
        {
            var services = new ServiceCollection();
            var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

            httpContext.Invoking(y => y.PerformSecurityChecks("policy")).Should().Throw<InvalidOperationException>();
        }

        [Fact, IsLayer0]
        public void When_IAuthorizationPolicyProviderNotRegistered_ShouldThrowInvalidOperationException()
        {
            var services = new ServiceCollection();
            services.AddTransient(_ => Mock.Of<IPolicyEvaluator>());
            var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

            httpContext.Invoking(y => y.PerformSecurityChecks("policy")).Should().Throw<InvalidOperationException>();
        }

        [Fact, IsLayer0]
        public void When_Challenged_ShouldNotThrow()
        {
            var services = new ServiceCollection();

            var provider = new Mock<IAuthorizationPolicyProvider>();
            provider.Setup(pr => pr.GetPolicyAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationPolicy(new[] { new NameAuthorizationRequirement("blah") }, new string[0]));
            var evaluator = new Mock<IPolicyEvaluator>();
            evaluator.Setup(ev => ev.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()))
                .ReturnsAsync(PolicyAuthorizationResult.Challenge);
            evaluator.Setup(ev => ev.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
               .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "scheme")));

            services.AddTransient(_ => provider.Object);
            services.AddTransient(_ => evaluator.Object);
            services.AddTransient(_ => Mock.Of<IAuthenticationService>());
            var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

            httpContext.Invoking(y => y.PerformSecurityChecks("policy")).Should().NotThrow();
        }

        [Fact, IsLayer0]
        public void When_Forbid_ShouldNotThrow()
        {
            var services = new ServiceCollection();

            var provider = new Mock<IAuthorizationPolicyProvider>();
            provider.Setup(pr => pr.GetPolicyAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationPolicy(new[] { new NameAuthorizationRequirement("blah") }, new string[0]));
            var evaluator = new Mock<IPolicyEvaluator>();
            evaluator.Setup(ev => ev.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<AuthenticateResult>(), It.IsAny<HttpContext>(), It.IsAny<object>()))
                .ReturnsAsync(PolicyAuthorizationResult.Forbid);
            evaluator.Setup(ev => ev.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), It.IsAny<HttpContext>()))
              .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "scheme")));

            services.AddTransient(_ => provider.Object);
            services.AddTransient(_ => evaluator.Object);
            services.AddTransient(_ => Mock.Of<IAuthenticationService>());
            var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

            httpContext.Invoking(y => y.PerformSecurityChecks("policy")).Should().NotThrow();
        }
    }
}
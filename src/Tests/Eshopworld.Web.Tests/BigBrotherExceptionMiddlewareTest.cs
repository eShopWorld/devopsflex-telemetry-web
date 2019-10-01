using System.Net;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;

namespace Eshopworld.Web.Tests
{
    using System;
    using FluentAssertions;
    using Eshopworld.Tests.Core;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using System.Threading.Tasks;
    using Web;
    using Core;

    // ReSharper disable once CheckNamespace
    public class BigBrotherExceptionMiddlewareTest
    {
        [Fact, IsLayer0]
        public void Ensure_CtorThrows_WithNullBigBrother()
        {
            var blewUp = false;
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new BigBrotherExceptionMiddleware(null, null);
            }
            catch (ArgumentNullException e)
            {
                e.Message.Should().Contain(nameof(IBigBrother));
                blewUp = true;
            }

            blewUp.Should().BeTrue();
        }

        [Fact, IsLayer0]
        public async Task Default_HttpStatusCode_On_Exception_Is_InternalServerError()
        {
            var middleware = new BigBrotherExceptionMiddleware(context => throw new Exception(), Mock.Of<IBigBrother>());
            var httpContext = new DefaultHttpContext();
            
            
            await middleware.Invoke(httpContext);


            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact, IsLayer0]
        public async Task HttpStatusCode_On_Exception_Is_The_One_Passed_On_Ctor()
        {
            var statusCode = HttpStatusCode.ServiceUnavailable;

            var middleware = new BigBrotherExceptionMiddleware(context => throw new Exception(), Mock.Of<IBigBrother>(), statusCode);
            var httpContext = new DefaultHttpContext();
            
            
            await middleware.Invoke(httpContext);


            httpContext.Response.StatusCode.Should().Be((int)statusCode);
        }



        public class Invoke
        {
            [Fact, IsLayer0]
            public async Task Test_ExceptionsAreHandled()
            {
                var mockMiddleware = new Mock<BigBrotherExceptionMiddleware>(MockBehavior.Loose,
                    new RequestDelegate(_ => throw new Exception("KABUM")), new Mock<IBigBrother>().Object, null);
                mockMiddleware.Setup(x => x.Invoke(It.IsAny<HttpContext>())).CallBase();
                mockMiddleware.Setup(x => x.HandleException(It.IsAny<HttpContext>(), It.IsAny<Exception>()))
                    .Returns(Task.CompletedTask);

                await mockMiddleware.Object.Invoke(new Mock<HttpContext>().Object);

                mockMiddleware.Verify(x => x.HandleException(It.IsAny<HttpContext>(), It.IsAny<Exception>()),
                    Times.Once);
            }

            [Fact, IsLayer0]
            public async Task Test_HandlesResponseStarted_GenericException()
            {
                var mockMiddleware = new BigBrotherExceptionMiddleware(_ => throw new Exception("KABUM"),
                    new Mock<IBigBrother>().Object);

                var mockHttpContext = new Mock<HttpContext>();
                mockHttpContext.SetupGet(context => context.Response.HasStarted)
                    .Returns(true); //mock response already started
                mockHttpContext.SetupSet<int>(context => context.Response.StatusCode = It.IsAny<int>())
                    .Throws<InvalidOperationException>();

                await mockMiddleware.Invoke(mockHttpContext.Object);

            }

            [Fact, IsLayer0]
            public async Task Test_HandlesResponseStarted_BadRequestException()
            {
                var mockMiddleware = new BigBrotherExceptionMiddleware(_ => throw new BadRequestException(),
                    new Mock<IBigBrother>().Object);

                var mockHttpContext = new Mock<HttpContext>();
                mockHttpContext.SetupGet(context => context.Response.HasStarted)
                    .Returns(true); //mock response already started
                mockHttpContext.SetupSet<int>(context => context.Response.StatusCode = It.IsAny<int>())
                    .Throws<InvalidOperationException>();

                await mockMiddleware.Invoke(mockHttpContext.Object);

            }
        }
    }
}

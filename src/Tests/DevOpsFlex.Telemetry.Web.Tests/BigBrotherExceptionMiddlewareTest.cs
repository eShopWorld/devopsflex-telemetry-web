using System;
using DevOpsFlex.Telemetry;
using DevOpsFlex.Telemetry.Web;
using DevOpsFlex.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
public class BigBrotherExceptionMiddlewareTest
{
    [Fact, IsUnit]
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

    public class Invoke
    {
        //[Fact, IsUnit]
        public async Task Test_ExceptionsAreHandled()
        {
            var mockMiddleware = new Mock<BigBrotherExceptionMiddleware>(MockBehavior.Loose, new RequestDelegate(_ => throw new Exception("KABUM")),
                new Mock<IBigBrother>().Object) {CallBase = true};
            mockMiddleware.Setup(x => x.Invoke(It.IsAny<HttpContext>())).CallBase();
            mockMiddleware.Setup(x => x.HandleExceptionAsync(It.IsNotNull<HttpContext>(), It.IsNotNull<Exception>())).Throws(new Exception());

            await mockMiddleware.Object.Invoke(new Mock<HttpContext>().Object);

            mockMiddleware.Verify(x => x.HandleExceptionAsync(It.IsNotNull<HttpContext>(), It.IsNotNull<Exception>()), Times.Once);
        }
    }
}

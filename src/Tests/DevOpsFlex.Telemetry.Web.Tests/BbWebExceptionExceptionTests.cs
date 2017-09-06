using System;
using DevOpsFlex.Tests.Core;
using FluentAssertions;
using Xunit;

namespace DevOpsFlex.Telemetry.Web.Tests
{
    public class BbWebExceptionExceptionTests
    {
        [Fact, IsUnit]
        public void Test_ConversionToBbEvent_CheckResponseAlreadyStarted()
        {
            var exception = new ApplicationException("BOOM");
            var product = exception.ToWebBbEvent(true);

            product.ResponseAlreadyStarted.Should().BeTrue();
        }
    }
}

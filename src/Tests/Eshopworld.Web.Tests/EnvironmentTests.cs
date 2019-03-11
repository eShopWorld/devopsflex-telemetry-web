using System;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class EnvironmentTests
    {
        [Theory, IsLayer0]
        [InlineData("test_ApplicationName", false)]
        [InlineData("Fabric_ApplicationName", true)]
        public void Test_CheckInServiceFabric(string variableName, bool expectation)
        {
            Environment.SetEnvironmentVariable(variableName, "hello app");

            EnvironmentHelper.IsInFabric.Should().Be(expectation);
            Assert.Equal(expectation, EnvironmentHelper.IsInFabric);
        }
    }
}

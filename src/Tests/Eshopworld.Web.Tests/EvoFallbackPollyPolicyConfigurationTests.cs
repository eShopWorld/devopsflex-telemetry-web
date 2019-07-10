using System;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class EvoFallbackPollyPolicyConfigurationTests
    {
        [Fact, IsLayer0]
        public void ValidateSuccess()
        {
            new EvoFallbackPollyPolicyConfiguration
                {
                    RetriesPerLevel = 1,
                    RetryTimeOut = TimeSpan.FromMilliseconds(1),
                    WaitTimeBetweenRetries = TimeSpan.FromSeconds(1)
                }
                .IsValid
                .Should().BeTrue();
        }

        [Fact, IsLayer0]
        public void ValidateFail()
        {
            new EvoFallbackPollyPolicyConfiguration()
                .IsValid
                .Should().BeFalse();
        }
    }
}

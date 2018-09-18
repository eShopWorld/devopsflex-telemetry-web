using DevOpsFlex.Tests.Core;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class EnvironmentTests
    {
        [Fact, IsUnit]
        public void Test_CheckInServiceFabric()
        {
            Assert.True(!EnvironmentHelper.IsInFabric);
        }

        [Fact, IsUnit]
        public void Test_CheckNotInServiceFabric()
        {
            Assert.False(EnvironmentHelper.IsInFabric);
        }
    }
}
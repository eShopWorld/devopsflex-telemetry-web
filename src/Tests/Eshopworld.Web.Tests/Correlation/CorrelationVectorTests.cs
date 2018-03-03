using System;
using System.Collections.Generic;
using System.Text;
using DevOpsFlex.Tests.Core;
using Eshopworld.Web.Correlation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace Eshopworld.Web.Tests.Correlation
{
    public class CorrelationVectorTests
    {
        [Theory, IsUnit]
        [InlineData("a.1", "a.1.1")]
        [InlineData("a.b.1", "a.b.1.1")]
        [InlineData("a.b.c.2", "a.b.c.2.1")]
        public void ToString_Success(string incoming, string expected)
        {
            var sut = new CorrelationVector(incoming);
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes(expected)), sut.ToString());
        }

        [Fact, IsUnit]
        public void Augment_Success()
        {
            var sut = new CorrelationVector("a.1");
            sut.Increase();
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("a.1.2")), sut.ToString());
            sut.Increase();
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("a.1.3")), sut.ToString());
        }
    }
}

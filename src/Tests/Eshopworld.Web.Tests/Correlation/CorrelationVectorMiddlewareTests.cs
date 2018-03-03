using System;
using System.Text;
using System.Threading.Tasks;
using DevOpsFlex.Tests.Core;
using Eshopworld.Web.Correlation;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Eshopworld.Web.Tests.Correlation
{
    /// <summary>
    /// tests for <see cref="CorrelationVectorMiddleware"/>
    /// </summary>
    public class CorrelationVectorMiddlewareTests
    {
        [Fact, IsUnit]
        public void TestHttpContextAugmentedWithNewCorrelationVector_Success()
        {
            var sut = new CorrelationVectorMiddleware(_ => Task.CompletedTask);
            var ctx = new DefaultHttpContext();

            sut.Invoke(ctx);
            Assert.Contains(ctx.Items, pair => (pair.Key as string)==CorrelationVector.CorrelationVectorHeaderName);
        }

        [Fact, IsUnit]
        public void IncomingHeaderWithCorrelationVectorConsumed_Success()
        {
            var sut = new CorrelationVectorMiddleware(_ => Task.CompletedTask);
            var ctx = new DefaultHttpContext();

            var guid = Guid.NewGuid().ToString();
            ctx.Request.Headers.Add(CorrelationVector.CorrelationVectorHeaderName, Convert.ToBase64String(Encoding.UTF8.GetBytes(guid)));
            sut.Invoke(ctx);
            var cv = ctx.GetCorrelationVector();
            Assert.NotNull(cv);
            Assert.Equal(Encoding.UTF8.GetString(Convert.FromBase64String(cv.ToString())), $"{guid}.1");
        }

        [Fact, IsUnit]
        public void TestNextDelegateIsCalled_Success()
        {
            bool called = false;
            var sut = new CorrelationVectorMiddleware(_ =>
            {
                called = true;
                return Task.CompletedTask;
            });
            var ctx = new DefaultHttpContext();
            sut.Invoke(ctx);
            Assert.True(called);

        }
    }
}

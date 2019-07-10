using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class EvoFallbackPollyPolicyHttpHandlerTests
    {
        [Fact, IsLayer0]
        public async Task TestFullCascadeFlow()
        {
            var mockHandler = new MockHttpMessageHandler();

            var testConfig = new EvoFallbackPollyPolicyConfigurationBuilder().SetRetriesPerLevel(2)
                .SetWaitTimeBetweenRetries(TimeSpan.FromMilliseconds(10)).SetRetryTimeOut(TimeSpan.FromSeconds(10))
                .Build();
            var eswRetryPolicy = EvoFallbackPollyPolicyBuilder.EswDnsRetryPolicy(testConfig);

            var handler = new EvoFallbackPollyPolicyHttpHandler(eswRetryPolicy, new TestDns())
            {
                InnerHandler = mockHandler
            };

            var mockedRequests = new List<MockedRequest>(new[]
            {
                mockHandler.Expect(HttpMethod.Get, "http://rp").Respond(HttpStatusCode.InternalServerError),
                mockHandler.Expect(HttpMethod.Get, "http://rp").Respond(HttpStatusCode.InternalServerError),
                mockHandler.Expect(HttpMethod.Get, "http://elb").Respond(HttpStatusCode.InternalServerError),
                mockHandler.Expect(HttpMethod.Get, "http://elb").Respond(HttpStatusCode.InternalServerError),
                mockHandler.Expect(HttpMethod.Get, "http://fd").Respond(HttpStatusCode.InternalServerError),
                mockHandler.Expect(HttpMethod.Get, "http://fd").Respond(HttpStatusCode.OK)

            });

            var httpClient = new HttpClient(handler);

            var result = await httpClient.GetAsync("http://blah");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            mockedRequests.TrueForAll(r => mockHandler.GetMatchCount(r) == 1);
        }

        [Fact, IsLayer0]
        public async Task Timeouts()
        {
            var testConfig = new EvoFallbackPollyPolicyConfigurationBuilder().SetRetriesPerLevel(2)
                .SetWaitTimeBetweenRetries(TimeSpan.FromMilliseconds(10)).SetRetryTimeOut(TimeSpan.FromMilliseconds(10))
                .Build();

            var eswRetryPolicy = EvoFallbackPollyPolicyBuilder.EswDnsRetryPolicy(testConfig);

            var handler = new EvoFallbackPollyPolicyHttpHandler(eswRetryPolicy, new TestDns())
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new HttpClient(handler);

            var result = await httpClient.GetAsync("http://blah");
            result.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        }

        [Fact, IsLayer0]
        public async Task NonTransientFailures()
        {
            var mockHandler = new MockHttpMessageHandler();

            var testConfig = new EvoFallbackPollyPolicyConfigurationBuilder().SetRetriesPerLevel(2)
                .SetWaitTimeBetweenRetries(TimeSpan.FromMilliseconds(10)).SetRetryTimeOut(TimeSpan.FromSeconds(10))
                .Build();

            var eswRetryPolicy = EvoFallbackPollyPolicyBuilder.EswDnsRetryPolicy(testConfig);

            var handler = new EvoFallbackPollyPolicyHttpHandler(eswRetryPolicy, new TestDns())
            {
                InnerHandler = mockHandler
            };

            var rp = mockHandler.Expect(HttpMethod.Get, "http://rp").Respond(HttpStatusCode.Redirect);
            var elb = mockHandler.Expect(HttpMethod.Get, "http://elb").Respond(HttpStatusCode.Redirect);
            var fd = mockHandler.Expect(HttpMethod.Get, "http://fd").Respond(HttpStatusCode.OK);

            var httpClient = new HttpClient(handler);

            var result = await httpClient.GetAsync("http://blah");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            mockHandler.GetMatchCount(rp).Should().Be(1); //go straight level up
            mockHandler.GetMatchCount(elb).Should().Be(1); //go straight level up
            mockHandler.GetMatchCount(fd).Should().Be(1); //go straight level up
        }

        [Fact, IsLayer0]
        public async Task ExceptionFailures()
        {
            var mockHandler = new MockHttpMessageHandler();

            var testConfig = new EvoFallbackPollyPolicyConfigurationBuilder().SetRetriesPerLevel(2)
                .SetWaitTimeBetweenRetries(TimeSpan.FromMilliseconds(10)).SetRetryTimeOut(TimeSpan.FromSeconds(10))
                .Build();

            var eswRetryPolicy = EvoFallbackPollyPolicyBuilder.EswDnsRetryPolicy(testConfig);

            var handler = new EvoFallbackPollyPolicyHttpHandler(eswRetryPolicy, new TestDns())
            {
                InnerHandler = mockHandler
            };

            var rp = mockHandler.Expect(HttpMethod.Get, "http://rp").Throw(new ApplicationException());
            var elb = mockHandler.Expect(HttpMethod.Get, "http://elb").Throw(new ApplicationException());
            var fd = mockHandler.When(HttpMethod.Get, "http://fd").Throw(new ApplicationException());
            
            var httpClient = new HttpClient(handler);

            var result = await httpClient.GetAsync("http://blah");
            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            mockHandler.GetMatchCount(rp).Should().Be(1); //go straight level up
            mockHandler.GetMatchCount(elb).Should().Be(1); //go straight level up
            mockHandler.GetMatchCount(fd).Should().Be(1); //straight level up
        }

        private class TestDns : IDnsConfigurationCascade
        {
            public string Proxy { get; set; } = "http://rp";
            public string Cluster { get; set; } = "http://elb";
            public string Global { get; set; } = "http://fd";
        }
    }

}

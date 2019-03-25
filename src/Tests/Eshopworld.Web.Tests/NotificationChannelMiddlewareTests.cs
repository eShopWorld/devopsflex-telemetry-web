using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eshopworld.Core;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Eshopworld.Tests.Core;
using EShopworld.Security.Services.Testing.Token;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class NotificationChannelMiddlewareTests
    {
        [Fact, IsLayer1]
        public async Task TestNotificationFlowMultipleSubscribers()
        {
            var token = await GetAccessToken();

            var f = new TestApiFactory();
            var cl = f.CreateClient();
            
            var observable = (NotificationObservableHost) f.Server.Host.Services.GetService(typeof(NotificationObservableHost));

            var signalA = new ManualResetEvent(false);
            observable.Subscribe<TestNotification>((n) => { signalA.Set(); });

            var signalB = new ManualResetEvent(false);
            observable.Subscribe<TestNotificationSubType>((n) => { signalB.Set(); });

            cl.SetBearerToken(token);

            var response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));

            response.IsSuccessStatusCode.Should().BeTrue();

            signalA.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
            signalB.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact, IsLayer1]
        public async Task TestNotificationFlowHotness()
        {
            var token = await GetAccessToken();

            var f = new TestApiFactory();
            var cl = f.CreateClient();

            var observable = (NotificationObservableHost)f.Server.Host.Services.GetService(typeof(NotificationObservableHost));

            var countForA = 0;
            var signalA = new ManualResetEvent(false);
            observable.Subscribe<TestNotification>((n) =>
            {
                signalA.Set();
                countForA++;
            });            

            cl.SetBearerToken(token);

            var response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.IsSuccessStatusCode.Should().BeTrue();

            var countForB = 0;
            var signalB = new ManualResetEvent(false);
            observable.Subscribe<TestNotificationSubType>((n) => { signalB.Set();
                countForB++;
            });

            response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.IsSuccessStatusCode.Should().BeTrue();

            signalA.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
            signalB.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();

            countForA.Should().Be(2);
            countForB.Should().Be(1);
        }


        [Fact, IsLayer1]
        public async Task TestNotificationCatchAllFlow()
        {
            var token = await GetAccessToken();

            var f = new TestApiFactory();
            var cl = f.CreateClient();

            var observable = (NotificationObservableHost)f.Server.Host.Services.GetService(typeof(NotificationObservableHost));

            int x=0;
            observable.SubscribeToAll((msg) => x++);

            cl.SetBearerToken(token);

            var response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotification,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotification()), Encoding.UTF8, "application/json"));
            response.IsSuccessStatusCode.Should().BeTrue();
            response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.IsSuccessStatusCode.Should().BeTrue();

            x.Should().Be(2);
        }

        [Fact, IsLayer0]
        public async Task TestForbidFlowNoPolicySet()
        {
            var nextDelegateMock = new Mock<RequestDelegate>();

            var mw = new NotificationChannelMiddleware(nextDelegateMock.Object, new NotificationChannelMiddlewareOptions(), 
                new NotificationObservableHost());

            var testContext = CreateTestHttpContext();
            await mw.Invoke(testContext);
            testContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }

        [Fact, IsLayer0]
        public async Task TestForbidFlowNonExistentPolicySet()
        {
            var nextDelegateMock = new Mock<RequestDelegate>();

            var testContext = CreateTestHttpContext();

            var mw = new NotificationChannelMiddleware(nextDelegateMock.Object, new NotificationChannelMiddlewareOptions() { AuthorizationPolicyName = "blah" },
                new NotificationObservableHost());

            await mw.Invoke(testContext);
            testContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }

        [Fact, IsLayer0]
        public async Task TestForbidFlowInvalidToken()
        {
            var nextDelegateMock = new Mock<RequestDelegate>();

            var testContext = CreateTestHttpContext();

            testContext.Request.Headers.Add("Authentication", new StringValues("Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkQwQTM4OTU4RjlEMjFGQkE1RTQ3RDg3N0MxMTA3MkM5Q0MwQzdERUEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiIwS09KV1BuU0g3cGVSOWgzd1JCeXljd01mZW8ifQ.eyJuYmYiOjE1NTE5NTI4NjgsImV4cCI6MTU1MTk1NjQ2OCwiaXNzIjoiaHR0cHM6Ly9zZWN1cml0eS1zdHMuY2kuZXNob3B3b3JsZC5uZXQiLCJhdWQiOlsiaHR0cHM6Ly9zZWN1cml0eS1zdHMuY2kuZXNob3B3b3JsZC5uZXQvcmVzb3VyY2VzIiwic2llcnJhLmFwaSJdLCJjbGllbnRfaWQiOiJlc3cuc2llcnJhLmFwaS50ZXN0LmNsaWVudCIsInNjb3BlIjpbImVzdy5zaWVycmEuYXBpLmFsbCJdfQ.nM7tDRDaA8mhCY6eyOqAFjFvnMTY0u49hFAj8lwsXk6KfbD_SOcVmaw9r90g95B38OAJ2WHS536mZjQjZh6QSWIu2nkLJqyDcInEuS77Yxu0nYOX6x4lmqB5D-XY8J4zBl0BA7KHC1-MSJ6VSNP90RF9903V9eMxIN0c_fV9pgU7Asqq86TiU8a9Szug-0EoW-kkcO_zFUCt-IzOEe-HDzY2kFVrxGZuPIptmOcUKlB_kL8SeSgScQggEefEHV-48zQ3yQPyfVo-8vt4-dgcCHVZ76upYAXJnlDvhuVHCnK30QfirtmU6cDZ2Mq1RfdP1z-quxIrAdEAzU2KoXGYxQ"));

            var mw = new NotificationChannelMiddleware(nextDelegateMock.Object, new NotificationChannelMiddlewareOptions() { AuthorizationPolicyName = "AssertScope" },
                new NotificationObservableHost());

            await mw.Invoke(testContext);
            testContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }

        [Fact, IsLayer1]
        public async Task TestForbidFlowValidToken()
        {
            var token = await GetAccessToken(true);

            var f = new TestApiFactory();
            var cl = f.CreateClient();
            cl.SetBearerToken(token);

            var response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact, IsLayer1]
        public async Task TestForbidFlowValidTokenRealController()
        {
            var token = await GetAccessToken(true);

            var f = new TestApiFactory();
            var cl = f.CreateClient();
            cl.SetBearerToken(token);

            var response = await cl.GetAsync("/api/values");
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact, IsLayer1]
        public async Task TestForbidFlowValidTokenWithoutRealApiClaim()
        {
            var token = await GetAccessToken();

            var f = new TestApiFactory();
            var cl = f.CreateClient();
            cl.SetBearerToken(token);

            var response = await cl.GetAsync("/api/values");
            response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }

        [Fact, IsLayer1]
        public async Task TestForbidFlowExpiredToken()
        {
            var f = new TestApiFactory();
            var cl = f.CreateClient();
            cl.SetBearerToken("eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2Iiwia2lkIjoiRDBBMzg5NThGOUQyMUZCQTVFNDdEODc3QzExMDcyQzlDQzBDN0RFQSIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1NTE5ODI4MTAsImV4cCI6MTU1MTk4NjQxMCwiaXNzIjoiaHR0cHM6Ly9zZWN1cml0eS1zdHMuY2kuZXNob3B3b3JsZC5uZXQiLCJhdWQiOlsiaHR0cHM6Ly9zZWN1cml0eS1zdHMuY2kuZXNob3B3b3JsZC5uZXQvcmVzb3VyY2VzIiwiZXN3LnRvb2xpbmdJbnRUZXN0Il0sImNsaWVudF9pZCI6ImVzdy50b29saW5nSW50VGVzdENsaWVudCIsInN1YiI6ImJsYWgiLCJpZHAiOiJvaWRjLWF6dXJlIiwiU2NvcGUiOlsib3BlbmlkIiwicHJvZmlsZSIsImVzdy50b29saW5nSW50VGVzdCJdLCJhbXIiOlsiZXh0ZXJuYWwiXX0.L31m8k-jDlJdFgEU0XQ26aPZ5iQ0_hEYJIyyfQ36JDsGCGngcyy2eBAXt7GY4IUNuO8MWGvXpYSkn5iMsHIQBQ8o7wUV4mqXaeP2w3QFOx9cbKsqIMW6tFDlSu23oQIFP50zRxb9LEQBHrb_CYA6VfZD8ilW2A59nKjtus9SQsukJCzYYjOwgmzty7DTzwRXUfe7ox0RLseD2FIavN6AY47YSGpgZv5arXzN5yqE3RQfSMkfQcGUJZtVhXMUk_1ZelTIwPrwIMPdM74fi8uEjVY3m9bhRV8F8NaPPr1QoRyYTGxeR2yV-CfgiqaLKdg3VoQp8KOQIDzNYOYpWcaVzQ");

            var response = await cl.PostAsync("/notification/Eshopworld.Web.Tests.NotificationChannelMiddlewareTests+TestNotificationSubType,Eshopworld.Web.Tests", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }

        [Fact, IsLayer1]
        public async Task UnrecognizedTypeReturnsBadRequest()
        {
            var f = new TestApiFactory();
            var cl = f.CreateClient();
            cl.SetBearerToken(await GetAccessToken());

            var response = await cl.PostAsync("/notification/blah", new StringContent(JsonConvert.SerializeObject(new TestNotificationSubType()), Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Be("Type 'blah' cannot be resolved");
        }

        private HttpContext CreateTestHttpContext()
        {
            var ctx = new DefaultHttpContext();

            var startup = new TestStartup();
            var sc = new ServiceCollection();

            startup.ConfigureServices(sc);
            ctx.RequestServices = sc.BuildServiceProvider();

            return ctx;
        }

        private async Task<string> GetAccessToken(bool includeRealApiClaim = false)
        {
            var config = EswDevOpsSdk.BuildConfiguration();

            var generator = new TokenGenerator();

            var certificate = new X509Certificate2(Convert.FromBase64String(config["STSSigningCert"]), "");
            var issuer = config["STSIssuer"];

            var claims = new List<Claim>
            {
                new Claim("client_id", "esw.toolingIntTestClient"),
                new Claim("sub", "blah"),
                new Claim("idp", "oidc-azure"),
                new Claim("Scope", "openid"),
                new Claim("Scope", "profile"),
                new Claim("Scope", "esw.toolingIntTest"),

                new Claim("amr", "external")
            };

            if (includeRealApiClaim)
            {
                claims.Add(new Claim("Scope", "esw.toolingInt"));
            }

            return await generator.CreateAccessTokenAsync($"{issuer}", new List<string>
            {
                $"{issuer}/resources",
                "esw.toolingIntTest"
            }, certificate, 3600, claims);
        }

        public class TestNotification
        {
            public string TestContent = "blah";
        }

        public class TestNotificationSubType : TestNotification
        {
            public string AdditionalContent = "blah blah";
        }

        public class TestStartup
        {
            
            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {

                services.AddAuthorization(options =>
                {
                    options.AddPolicy("AssertScope", policy =>
                        policy.RequireClaim("scope", "esw.toolingInt"));

                    options.AddPolicy("AssertTestScope", policy =>
                        policy.RequireClaim("scope", "esw.toolingIntTest"));
                });

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddIdentityServerAuthentication(
                    x =>
                    {
                        x.ApiName = "esw.toolingIntTest";
                        x.Authority = "https://security-sts.ci.eshopworld.net";
                    });

                services.AddMvc(options =>
                    {
                        var policy = ScopePolicy.Create("esw.toolingInt");
                        options.Filters.Add(new AuthorizeFilter(policy));
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                services.AddLogging();
                services.Add(new ServiceDescriptor(typeof(IBigBrother), Mock.Of<IBigBrother>()));
                services.Add(new ServiceDescriptor(typeof(NotificationObservableHost), new NotificationObservableHost()));
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseAuthentication();
                app.UseMiddleware<NotificationChannelMiddleware>(new NotificationChannelMiddlewareOptions
                    { AuthorizationPolicyName = "AssertTestScope" });

                app.UseMvc();
            }
        }

        public class TestApiFactory : WebApplicationFactory<ActorLayerTestMiddlewareTests.TestStartup>
        {
            protected override IWebHostBuilder CreateWebHostBuilder()
            {
                return new WebHostBuilder()
                    .UseStartup<TestStartup>();
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseContentRoot(".");

                base.ConfigureWebHost(builder);
            }
        }
    }
}

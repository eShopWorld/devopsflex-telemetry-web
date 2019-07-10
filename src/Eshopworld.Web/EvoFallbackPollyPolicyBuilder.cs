using System;
using System.Net;
using System.Net.Http;
using Microsoft.Rest;
using Polly;
using Polly.Timeout;

namespace Eshopworld.Web
{
    /// <summary>
    /// builder for <see cref="IAsyncPolicy{TResult}"/> that implements the Evolution fallback policy across different routes to the service
    /// </summary>
    public static class EvoFallbackPollyPolicyBuilder
    {
        public static IAsyncPolicy<HttpResponseMessage> EswDnsRetryPolicy(EvoFallbackPollyPolicyConfiguration config)
        {
            if (!config.Validate())
            {
                throw new ArgumentException("Invalid configuration passed", nameof(config));
            }

            return Policy<HttpResponseMessage>
                .Handle<Exception>(r => true)
                .WaitAndRetryForeverAsync((i, exception, arg3) => config.WaitTimeBetweenRetries,
                    async (exception, i, ts, ctx) =>
                    {
                        ctx.SetLevelRetryCount(config.RetriesPerLevel);

                        switch (exception?.Exception)
                        {
                            case TimeoutRejectedException _:
                                ctx.SetResponseCode(HttpStatusCode.RequestTimeout);
                                break;
                            case HttpOperationException opEx when opEx.Response != null:
                                ctx.SetResponseCode(opEx.Response.StatusCode);
                                break;
                        }

                        if (!ctx.IsTransient())
                        {
                            ctx.LevelUp(); //straight to next level
                        }
                        else
                        {
                            ctx.AnotherAttempt();
                        }
                    })
                .WrapAsync(Policy.TimeoutAsync((context => config.RetryTimeOut)));
        }
    }
}

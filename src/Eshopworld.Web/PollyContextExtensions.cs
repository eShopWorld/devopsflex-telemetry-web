using System;                                                   
using System.Net;
using Eshopworld.Core;
using Polly;

namespace Eshopworld.Web
{
    public static class PollyContextExtensions
    {
        private const string ResponseErrorCode = "EvoFallbackPolicyResponseErrorCode";
        private const string LevelIndex = "EvoFallbackPolicyLevelIndex";
        private const string LevelCount = "EvoFallbackPolicyLevelCount";
        private const string LevelRetryCount = "EvoFallbackPolicyLevelRetryCount";

        private static string GetLevelContextKey(int level) => $"EvoFallbackPolicyLevel{level}";

        public static void SetResponseCode(this Context ctx, HttpStatusCode responseStatusCode)
        {
            ctx[ResponseErrorCode] = responseStatusCode;
        }

        public static HttpStatusCode GetLastResponseErrorCode(this Context ctx)
        {
            return ctx.ContainsKey(ResponseErrorCode) ? (HttpStatusCode)ctx[ResponseErrorCode] : HttpStatusCode.InternalServerError;
        }

        public static Uri GetUrl(this Context ctx)
        {
            var level = (int)ctx[LevelIndex];

            return new Uri((string)ctx[GetLevelContextKey(level)]);
        }

        public static void AnotherAttempt(this Context ctx)
        {
            if (!ctx.ContainsKey(LevelCount))
            {
                ctx[LevelCount] = 0;
            }

            var currentCount = (int)ctx[LevelCount] + 1;
            if (currentCount < ctx.GetLevelRetryCount())
            {
                ctx[LevelCount] = currentCount;
            }
            else
            {
                LevelUp(ctx);
            }
        }

        public static void LevelUp(this Context ctx)
        {
            ctx[LevelCount] = 0;
            if (!ctx.ContainsKey(LevelIndex))
            {
                ctx[LevelIndex] = 0;
            }

            ctx[LevelIndex] = (int)ctx[LevelIndex] + 1;
        }

        public static bool EndReached(this Context ctx)
        {
            var nextLevel = (int)ctx[LevelIndex] + 1;
            return !ctx.ContainsKey(GetLevelContextKey(nextLevel)) && (int)ctx[LevelCount] == ctx.GetLevelRetryCount() - 1;
        }

        public static bool PolicyExceeded(this Context ctx)
        {
            return !ctx.ContainsKey(GetLevelContextKey((int)ctx[LevelIndex])) && (int)ctx[LevelCount] == 0;
        }

        public static bool IsTransient(this Context ctx)
        {
            if (!ctx.ContainsKey(ResponseErrorCode))
                return false;

            var respStatusCode = (HttpStatusCode)ctx[ResponseErrorCode];

            return respStatusCode >= HttpStatusCode.InternalServerError || respStatusCode == HttpStatusCode.RequestTimeout;
        }

        public static void GenerateContextForLevels(this Context ctx, IDnsConfigurationCascade dns)
        {
            var level = 0;

            void AddLevel(string val)
            {
                ctx[GetLevelContextKey(level)] = val;
                level++;
            }

            if (!string.IsNullOrWhiteSpace(dns.Proxy))
            {
                AddLevel(dns.Proxy);
            }

            if (!string.IsNullOrWhiteSpace(dns.Cluster))
            {
                AddLevel(dns.Cluster);
            }

            if (!string.IsNullOrWhiteSpace(dns.Global))
            {
                AddLevel(dns.Global);
            }

            if (!ctx.ContainsKey(LevelIndex))
                ctx[LevelIndex] = 0;

            if (!ctx.ContainsKey(LevelCount))
                ctx[LevelCount] = 0;
        }

        public static void SetLevelRetryCount(this Context ctx, int retryCount)
        {
            ctx[LevelRetryCount] = retryCount;
        }

        private static int GetLevelRetryCount(this Context ctx)
        {
            return (int)ctx[LevelRetryCount];
        }
    }
}

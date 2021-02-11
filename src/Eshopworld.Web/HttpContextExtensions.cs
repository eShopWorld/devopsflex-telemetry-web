using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Eshopworld.Web
{
    /// <summary>
    /// some helpful extensions for <see cref="HttpContext"/>
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// allow for security checks to be performed (outside of MVC context e.g. top level middleware)
        /// </summary>
        /// <param name="ctx">http context instance</param>
        /// <param name="policyName">policy to run check against</param>
        /// <returns>true if succeeded, otherwise false (sets http response)</returns>
        public static async Task<bool> PerformSecurityChecks(this HttpContext ctx, string policyName)
        {
            var policyEvaluator = (IPolicyEvaluator)ctx.RequestServices.GetService(typeof(IPolicyEvaluator));

            if (policyEvaluator == null)
            {
                throw new InvalidOperationException($"Unable to obtain {typeof(IPolicyEvaluator)} from the container");
            }

            var policyProvider = (IAuthorizationPolicyProvider)ctx.RequestServices.GetService(typeof(IAuthorizationPolicyProvider));

            if (policyProvider == null)
            {
                throw new InvalidOperationException($"Unable to obtain {typeof(IAuthorizationPolicyProvider)} from the container");
            }

            if (string.IsNullOrWhiteSpace(policyName))
            {
                await ctx.ForbidAsync();
                return false;
            }

            //test security
            var policy = await policyProvider.GetPolicyAsync(policyName);
            if (policy == null)
            {
                await ctx.ForbidAsync(); //there is no point in retrying
                return false;
            }

            //authentication checks
            var authenticationResult = await policyEvaluator.AuthenticateAsync(policy, ctx);

            if (!authenticationResult.Succeeded)
            {
                await ctx.ChallengeAsync();
                return false;
            }

            //authorization checks
            var authorizationResult = await policyEvaluator.AuthorizeAsync(policy, authenticationResult, ctx, null);

            if (authorizationResult.Challenged)
            {
                await ctx.ChallengeAsync();
                return false;
            }

            if (!authorizationResult.Forbidden) return true;

            await ctx.ForbidAsync();
            return false;

        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eshopworld.Core;
using Microsoft.Extensions.Http;
using Polly;

namespace Eshopworld.Web
{
    /// <summary>
    /// custom Polly delegating handler, wrapping policy and Evolution DNS call cascade
    ///
    /// it handles workflow of the policy itself and relies heavily on <see cref="Context"/>
    /// for error flows, <see cref="EvoFallbackPollyPolicyBuilder"/> creates policy that handles errors/exceptions and sets context accordingly
    ///
    /// the actual HTTP comm would be performed by inner handler (usually <see cref="HttpClientHandler"/>)
    /// </summary>
    public class EvoFallbackPollyPolicyHttpHandler : PolicyHttpMessageHandler
    {
        private readonly IDnsConfigurationCascade _config;

        /// <summary>
        /// constructor with policy given explicitly
        /// </summary>
        /// <param name="policy">Polly policy instance</param>
        /// <param name="config">DNS configuration instance</param>
        public EvoFallbackPollyPolicyHttpHandler(IAsyncPolicy<HttpResponseMessage> policy, IDnsConfigurationCascade config) : base(policy)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// constructor with policy selector
        /// </summary>
        /// <param name="policySelector">policy selector</param>
        /// <param name="config">DNS configuration instance</param>
        public EvoFallbackPollyPolicyHttpHandler(Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector, IDnsConfigurationCascade config) : base(policySelector)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        protected override async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, Context context, CancellationToken cancellationToken)
        {
            context.GenerateContextForLevels(_config);
            if (context.PolicyExceeded()) //this handles policy failures outside of response (and codes) as in some flow there is no response (timeouts being good example)
            {
                return new HttpResponseMessage(context.GetLastResponseErrorCode());
            }

            var targetUri = new UriBuilder(context.GetUrl())
            {
                Path = request.RequestUri.PathAndQuery
            };
            request.RequestUri = targetUri.Uri;
            var resp = await base.SendCoreAsync(request, context, cancellationToken);

            if (IsAcceptedStatusCode(resp.StatusCode))
                return resp;

            context.SetResponseCode(resp.StatusCode);

            if (!context.EndReached())
                resp.EnsureSuccessStatusCode(); //back to the policy

            return resp; //no more tries, return last response
        }

        private static bool IsAcceptedStatusCode(HttpStatusCode responseStatusCode)
        {
            //2XX+4XX = Success family + client error family
            return (responseStatusCode >= HttpStatusCode.OK 
                    && responseStatusCode < HttpStatusCode.Ambiguous) 
                        || (responseStatusCode >= HttpStatusCode.BadRequest 
                            && responseStatusCode < HttpStatusCode.InternalServerError);
        }


    }

}

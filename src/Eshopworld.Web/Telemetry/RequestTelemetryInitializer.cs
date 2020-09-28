using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Eshopworld.Core;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Eshopworld.Web.Telemetry
{
    /// <summary>Allows interception of AI RequestTelemetry events and exposes request body</summary>
    [ExcludeFromCodeCoverage]
    public class RequestTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBigBrother _bigBrother;

        /// <summary>Default ctor</summary>
        /// <param name="httpContextAccessor">HttpContextAccessor to resolve the http request</param>
        /// <param name="bigBrother">BigBrother used for logging</param>
        public RequestTelemetryInitializer([JetBrains.Annotations.NotNull] IHttpContextAccessor httpContextAccessor, [JetBrains.Annotations.NotNull] IBigBrother bigBrother)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
        }

        /// <inheritdoc />
        public void Initialize(ITelemetry telemetry)
        {
            if (!(telemetry is RequestTelemetry requestTelemetry)) return;
            if (_httpContextAccessor?.HttpContext?.Request == null) return;

            var request = _httpContextAccessor.HttpContext.Request;

            try
            {
                if (string.IsNullOrEmpty(requestTelemetry.ResponseCode))
                {
                    HandleIncomingRequest(request.Path.Value, requestTelemetry, request);
                }
                else
                {
                    HandleCompletedRequest(request.Path.Value, requestTelemetry, request);
                }
            }
            catch (Exception e)
            {
                _bigBrother.Publish(e.ToExceptionEvent());
            }
        }

        /// <summary>Method is invoked for every <see cref="RequestTelemetry"/> with its associated <see cref="HttpRequest"/>, before it is handled by asp.net core pipeline</summary>
        /// <param name="path">Path of the current request</param>
        /// <param name="requestTelemetry">AI RequestTelemetry for the request</param>
        /// <param name="request">HttpRequest for the request</param>
        /// <remarks>Use this method to add telemetry detail before the request is handled. i.e. inbound request</remarks>
        protected virtual void HandleIncomingRequest(string path, RequestTelemetry requestTelemetry, HttpRequest request) { }

        /// <summary>Method is invoked for every <see cref="RequestTelemetry"/> with its associated <see cref="HttpRequest"/>, after it has been handled by the asp.net core pipeline</summary>
        /// <param name="path">Path of the current request</param>
        /// <param name="requestTelemetry">AI RequestTelemetry for the request</param>
        /// <param name="request">HttpRequest for the request</param>
        /// <remarks>Use this method to add telemetry detail after the request is handled. i.e. outbound request aka response</remarks>
        protected virtual void HandleCompletedRequest(string path, RequestTelemetry requestTelemetry, HttpRequest request) { }

        /// <summary>Helper method which will read the request body, deserialize to supplied type, and apply the selector function to the response</summary>
        /// <typeparam name="T">Type to deserialize the request body to</typeparam>
        /// <param name="request">Request to read body from</param>
        /// <param name="selector">Callback to project the request body type into a string. If the projection function returns a null reference, then the <paramref name="default"/> value will be applied</param>
        /// <param name="default">Value which will be applied if the selector callback returns null</param>
        /// <returns>string which represents the a body detail</returns>
        /// <remarks>Calling this method converts the request body to a rewindable stream. The method will first read the body from the stream, process it via the selector, and then rewind the stream for downstream handling</remarks>
        protected static string ExtractBodyDetail<T>(HttpRequest request, Func<T, string> selector, string @default = "(null)") => selector(DeserializeRequestBody<T>(request)) ?? @default;

        /// <summary>Helper method which will read the request body, deserialize to supplied type</summary>
        /// <typeparam name="T">Type to deserialize the request body to</typeparam>
        /// <param name="request">Request to read body from</param>
        /// <returns>string which represents the a body detail</returns>
        /// <remarks>Calling this method converts the request body to a rewindable stream. The method will first read the body from the stream, process it via the selector, and then rewind the stream for downstream handling</remarks>
        protected static T DeserializeRequestBody<T>(HttpRequest request)
        {
            // EnableBuffering is the equivalent of EnableRewind in asp.net core 3.1
            request.EnableBuffering();
            try
            {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
            finally
            {
                request.Body.Position = 0;
            }
        }
    }
}
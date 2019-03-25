using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Eshopworld.Web
{
    /// <summary>
    /// various options to configure the notification middleware
    /// </summary>
    public class NotificationChannelMiddlewareOptions
    {
        /// <summary>
        /// url route prefix to denote notification channel
        /// note that we are intentionally not hooking up to any /api route e.g. /api/notification
        /// </summary>
        public string UrlPrefix { get; set; } = "/notification";   
        /// <summary>
        /// name of the authorization policy to apply
        /// </summary>
        public string AuthorizationPolicyName { get; set; }
        /// <summary>
        /// (de)serialization settings to use, null denotes defaults will be applied inline with newtonsoft internal implementation
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
    }

    /// <summary>
    /// the notification middleware
    ///
    /// this requires an instance of <see cref="NotificationObservableHost"/>, which notification observers hook up to/subscribe for specific events
    /// </summary>
    public class NotificationChannelMiddleware
    {
        private readonly RequestDelegate _delegate;
        private readonly NotificationChannelMiddlewareOptions _options;
        private readonly NotificationObservableHost _observable;

        public NotificationChannelMiddleware(RequestDelegate @delegate, NotificationChannelMiddlewareOptions options, NotificationObservableHost observable)
        {
            _delegate = @delegate;
            _options = options;
            _observable = observable;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!await context.PerformSecurityChecks(_options.AuthorizationPolicyName))
            {
                return;
            }

            var isNotification = context.Request.Path.StartsWithSegments(
                _options.UrlPrefix,
                StringComparison.OrdinalIgnoreCase,
                out var remaining);
            
            if (isNotification)
            {
                //parse out fully qualified typename and assembly - we effectively expect the c# type resolver notation
                //structurally this is - Namespace.ContainingClass(+NestedClass)(,MyAssembly) - nested class and assembly designation are optional parts
                
                Type resolvedNotificationType;
                if (string.IsNullOrWhiteSpace(remaining.ToString()) || (resolvedNotificationType= Type.GetType(remaining.ToString().TrimStart('/'), false))==null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync($"Type '{remaining.ToString().TrimStart('/')}' cannot be resolved");

                    return;
                }

                using (var reader
                    = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var bodyStr = reader.ReadToEnd();
                    _observable.NewEvent(JsonConvert.DeserializeObject(bodyStr, resolvedNotificationType, _options.JsonSerializerSettings));
                }

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                return;
            }

            await _delegate.Invoke(context);
        }
    }
}

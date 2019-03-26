using System;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Eshopworld.Core;
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
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string UrlPrefix { get; set; } = "/notification";   
        /// <summary>
        /// name of the authorization policy to apply
        /// </summary>
        public string AuthorizationPolicyName { get; set; }
        /// <summary>
        /// (de)serialization settings to use, null denotes defaults will be applied inline with newtonsoft internal implementation
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
    }

    /// <summary>
    /// the notification middleware
    /// </summary>
    public class NotificationChannelMiddleware
    {
        internal  RequestDelegate Delegate { private get; set; }
        private readonly NotificationChannelMiddlewareOptions _options;
        private readonly Subject<BaseNotification> _subject  = new Subject<BaseNotification>(); 

        /// <summary>
        /// the observable entry-point allowing subscribing to notifications of various type
        /// </summary>
        public IObservable<BaseNotification> Observable => _subject.AsObservable();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options">options to configure this middleware</param>
        public NotificationChannelMiddleware(NotificationChannelMiddlewareOptions options)
        {            
            _options = options;            
        }

        /// <summary>
        /// main middleware logic
        /// </summary>
        /// <param name="context">http context</param>
        /// <returns>middleware result</returns>
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
                if (string.IsNullOrWhiteSpace(remaining.ToString()) || (resolvedNotificationType= Type.GetType(remaining.ToString().TrimStart('/'), false))==null || !resolvedNotificationType.IsSubclassOf(typeof(BaseNotification)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync($"Type '{remaining.ToString().TrimStart('/')}' cannot be resolved or is not subclass of {typeof(BaseNotification).FullName}");

                    return;
                }

                using (var reader
                    = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var bodyStr = reader.ReadToEnd();

                    _subject.OnNext((BaseNotification)JsonConvert.DeserializeObject(bodyStr, resolvedNotificationType,
                        _options.JsonSerializerSettings));
                }

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                return;
            }

            await Delegate.Invoke(context);
        }
    }
}

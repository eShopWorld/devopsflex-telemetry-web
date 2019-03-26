using System;
using Eshopworld.Core;
using Microsoft.AspNetCore.Builder;

namespace Eshopworld.Web
{
    /// <summary>
    /// various extension methods for <see cref="IApplicationBuilder"/>
    ///
    /// this allows for injection of <see cref="ActorLayerTestMiddleware"/> and <see cref="NotificationChannelMiddleware"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables handling of HTTP requests which are directly used to 
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">The middleware's parameters.</param>
        public static IApplicationBuilder UseActorLayerTestDirectCall(this IApplicationBuilder app, ActorLayerTestMiddlewareOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.StatelessServiceContext == null)
            {
                throw new ArgumentException("The StatelessServiceContext property must not be null", nameof(options));
            }

            if (options.PathPrefix.Value == null)
            {
                throw new ArgumentException("The PathPrefix property must not be null", nameof(options));
            }

            if (options.PathPrefix.Value.Length < 2 || options.PathPrefix.Value.EndsWith("/"))
            {
                throw new ArgumentException("The value of the PathPrefix property is invalid.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.AuthorizationPolicyName))
            {
                throw new ArgumentException("Authorization policy name must be set",
                    nameof(options.AuthorizationPolicyName));
            }

            return app.UseMiddleware<ActorLayerTestMiddleware>(options);
        }

        /// <summary>
        /// Enables handling of HTTP requests which - if notifications - are broadcasted via <see cref="IObservable{T}"/> exposed by this notification middleware
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">The middleware's parameters.</param>
        public static IObservable<BaseNotification> UseNotification(this IApplicationBuilder app,
            NotificationChannelMiddlewareOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AuthorizationPolicyName))
            {
                throw new ArgumentException("The value of the AuthorizationPolicyName property is invalid.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.UrlPrefix))
            {
                throw new ArgumentException("The value of the UrlPrefix property is invalid.", nameof(options));
            }

            var mw = new NotificationChannelMiddleware(options);

            app.Use((next) => {
                mw.Delegate = next;
                return mw.Invoke;
            });

            return mw.Observable;
        }
    }
}

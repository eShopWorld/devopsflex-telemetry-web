using System;
using System.Net;

namespace Eshopworld.Web
{
    using Core;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Contains the Use type middleware methods to register the <see cref="IBigBrother"/> exception handler.
    /// </summary>
    public static class BigBrotherMiddlewareExtensions
    {
        /// <summary>
        /// Register the <see cref="IBigBrother"/> exception handling middleware into the MVC pipeline.
        /// </summary>
        /// <param name="builder">The builder that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="responseHttpStatusCodeOnException">The <see cref="HttpStatusCode"/> we want to return in the response when handling an exception.</param>
        /// <returns>[FLUENT] the <paramref name="builder"/>.</returns>
        [Obsolete("Optional Status Code Response from the BigBrotherMiddleware will be removed in the next release. In favor of always returning HTTP 500")]
        public static IApplicationBuilder UseBigBrotherExceptionHandler(this IApplicationBuilder builder, HttpStatusCode responseHttpStatusCodeOnException)
        {
            return builder.UseMiddleware<BigBrotherExceptionMiddleware>(responseHttpStatusCodeOnException);
        }
        
        /// <summary>
        /// Register the <see cref="IBigBrother"/> exception handling middleware into the MVC pipeline.
        /// </summary>
        /// <param name="builder">The builder that provides the mechanisms to configure an application's request pipeline.</param>
        /// <returns>[FLUENT] the <paramref name="builder"/>.</returns>
        public static IApplicationBuilder UseBigBrotherExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BigBrotherExceptionMiddleware>();
        }
    }
}

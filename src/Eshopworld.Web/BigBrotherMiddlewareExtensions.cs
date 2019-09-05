using Eshopworld.Core;
using Microsoft.AspNetCore.Builder;

#nullable enable

namespace Eshopworld.Web
{
    /// <summary>
    /// Contains the Use type middleware methods to register the <see cref="IBigBrother"/> exception handler.
    /// </summary>
    public static class BigBrotherMiddlewareExtensions
    {
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

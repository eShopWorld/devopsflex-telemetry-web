using Microsoft.AspNetCore.Builder;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// extension class to inject correlation vector middleware to asp.net middleware builder
    /// </summary>
    public static class CorrelationVectorMiddlewareExtension
    {
        /// <summary>
        /// add correlation vector as use middleware
        /// </summary>
        /// <param name="builder">application builder</param>
        /// <returns>application builder</returns>
        public static IApplicationBuilder UseCorrelationVector(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationVectorMiddleware>();
        }

    }
}

using System;
using Microsoft.AspNetCore.Http;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// extension class for http context for easy access to correlation vector
    /// </summary>
    public  static class HttpContextExtension
    {
        /// <summary>
        /// get correlation vector extension method
        /// </summary>
        /// <param name="ctx">http context</param>
        /// <returns>correlation vector as as</returns>
        public static CorrelationVector GetCorrelationVector(this HttpContext ctx)
        {
            if (ctx.Items.TryGetValue(CorrelationVector.CorrelationVectorHeaderName, out var cv))
            {
                return cv as CorrelationVector;
            }
            else
            {
                throw new CorrelationVectorException("No correlation vector available on HttpContext, check your application middleware set up");
            }
        }
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// middleware class to manage correlation vector
    /// </summary>
    public class CorrelationVectorMiddleware
    {
        private readonly RequestDelegate _next;
        
        public CorrelationVectorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// middleware invocation point
        /// </summary>
        /// <param name="context">http context instance</param>
        /// <returns>[ASYNC] task representing middleware result</returns>
        public Task Invoke(HttpContext context)
        {          
            //setup correlation vector - either using the header or an empty one
            string header;
            CorrelationVector cv;
            if (!string.IsNullOrWhiteSpace(header = context.Request.Headers[CorrelationVector.CorrelationVectorHeaderName]))
            {                
                var decodedHeader = Encoding.UTF8.GetString(Convert.FromBase64String(header));
                cv = new CorrelationVector(decodedHeader);
                
            }
            else
            {
                cv = new CorrelationVector(Guid.NewGuid().ToString()); //new base id since no header was received
            }

            context.Items.Add(CorrelationVector.CorrelationVectorHeaderName, cv);

            //Call the next delegate/middleware in the pipeline
            return _next(context);
        }

    }
}


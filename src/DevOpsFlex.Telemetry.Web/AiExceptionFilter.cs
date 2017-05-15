namespace DevOpsFlex.Telemetry.Web
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Telemetry;

    /// <summary>
    /// A filter that runs after an action has thrown an <see cref="T:System.Exception" />.
    /// This specific filter streams the exception through the Application Insights client.
    /// </summary>
    public class AiExceptionFilter : IExceptionFilter
    {
        private readonly IBigBrother _bb;

        /// <summary>
        /// Initializes a new instance of <see cref="AiExceptionFilter"/>.
        /// </summary>
        /// <param name="bb"></param>
        public AiExceptionFilter(IBigBrother bb)
        {
            _bb = bb;
        }

        /// <summary>
        /// Called after an action has thrown an <see cref="T:System.Exception" />.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ExceptionContext" />.</param>
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is BadRequestException badRequest)
            {
                context.Result = new ObjectResult(badRequest.ToResponse())
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    DeclaredType = typeof(BadRequestErrorResponse)
                };
            }
            else
            {
                var response = new ErrorResponse
                {
                    Message = context.Exception.Message,
#if DEBUG
                    StackTrace = context.Exception.StackTrace
#endif
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                    DeclaredType = typeof(ErrorResponse)
                };
            }

            _bb.Publish(context.Exception.ToBbEvent());
        }
    }
}
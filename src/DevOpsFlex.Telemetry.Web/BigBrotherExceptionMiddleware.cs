namespace DevOpsFlex.Telemetry.Web
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// The middleware component that handles exceptions through <see cref="IBigBrother"/>.
    /// </summary>
    public class BigBrotherExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IBigBrother _bb;

        /// <summary>
        /// Initializes a new instance of <see cref="BigBrotherExceptionMiddleware"/>.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the pipeline.</param>
        /// <param name="bb">The <see cref="IBigBrother"/> that we want to stream exception telemetry to.</param>
        public BigBrotherExceptionMiddleware(RequestDelegate next, IBigBrother bb)
        {
            _next = next;
#if DEBUG
            _bb = bb ?? throw new ArgumentNullException(nameof(bb), $"{nameof(IBigBrother)} isn't registred as a service.");
#else
            _bb = bb;
#endif
        }

        /// <summary>
        /// Middleware entry point, called by the MVC pipeline.
        /// </summary>
        /// <param name="context">The HTTP-specific information about an individual HTTP request.</param>
        /// <returns>[ASYNC] <see cref="Task"/> future promise.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions by populating the response inside the <see cref="HttpContext"/> depending on
        ///     which type of exception is being handled.
        /// </summary>
        /// <param name="context">The HTTP-specific information about an individual HTTP request.</param>
        /// <param name="exception"></param>
        /// <returns>[ASYNC] <see cref="Task"/> future promise.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string result;

            if (exception is BadRequestException badRequest)
            {
                result = JsonConvert.SerializeObject(badRequest.ToResponse());
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                result = JsonConvert.SerializeObject(
                    new ErrorResponse
                    {
                        Message = exception.Message,
#if DEBUG
                        StackTrace = exception.StackTrace
#endif
                    });
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }

            _bb.Publish(exception.ToBbEvent());

            await context.Response.WriteAsync(result);
        }
    }
}
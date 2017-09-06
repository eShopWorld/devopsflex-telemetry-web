using System;
using DevOpsFlex.Core;

namespace DevOpsFlex.Telemetry.Web
{

    /// <summary>
    /// BigBrother base <see cref="System.Exception"/> tailored for web context to cover additional nuances
    /// </summary>
    public class BbWebExceptionEvent : BbExceptionEvent
    {
        public bool ResponseAlreadyStarted;
    }

    public static class BbWebExceptionEventExtensions
    {
        /// <summary>
        /// instantiate web exception BigBrother event using the raw exception
        /// this uses the inheritance between web and "plain" BigBrother exception
        /// </summary>
        /// <param name="exception">raw exception to convert into BigBrother event</param>
        /// <param name="responseHasStarted">flag indicating whether response has been started when processing pipeline before exception was caught</param>
        /// <returns></returns>
        public static BbWebExceptionEvent ToWebBbEvent(this Exception exception, bool responseHasStarted=false)
        {
            var exceptionEvent = exception.ToBbEvent<BbWebExceptionEvent>();
            exceptionEvent.ResponseAlreadyStarted = responseHasStarted;

            return exceptionEvent;
        }
    }
}

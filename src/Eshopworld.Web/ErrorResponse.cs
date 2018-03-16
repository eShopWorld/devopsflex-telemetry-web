namespace Eshopworld.Web
{
    /// <summary>
    /// This represents the response entity for error.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

#if DEBUG
        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        public string StackTrace { get; set; }
#endif
    }
}
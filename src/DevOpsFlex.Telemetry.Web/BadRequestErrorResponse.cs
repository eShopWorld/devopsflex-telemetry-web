namespace DevOpsFlex.Telemetry.Web
{
    using System.Collections.Generic;

    /// <summary>
    /// This represents the response entity for error thrown by <see cref="BadRequestException"/>.
    /// </summary>
    public class BadRequestErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Gets and sets the list of issues with parameters as part of this <see cref="BadRequestErrorResponse"/>.
        /// </summary>
        public IEnumerable<BadRequestParameter> Parameters { get; set; }
    }

    /// <summary>
    /// Represents a parameter problem in a <see cref="BadRequestErrorResponse"/>.
    /// </summary>
    public class BadRequestParameter
    {
        /// <summary>
        /// Gets and sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets and sets the description of the error with this parameter.
        /// </summary>
        public string Description { get; set; }
    }
}
namespace DevOpsFlex.Telemetry.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Web;
    using JetBrains.Annotations;

    /// <summary>
    /// Thrown when bad request parameters are passed into a WebAPI method.
    /// </summary>
    [Serializable]
    public sealed class BadRequestException : HttpException
    {
        internal readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of <see cref="BadRequestException"/>.
        /// </summary>
        /// <param name="method">The WebAPI method called with bad parameters.</param>
        public BadRequestException(string method)
            : base((int)HttpStatusCode.BadRequest, $"Bad request calling {method}, details are in {nameof(Parameters)}")
        { }

        /// <summary>
        /// Adds a parameter that shouldn't be null but is.
        /// </summary>
        /// <param name="parameter">The name of the parameter.</param>
        /// <returns>[FLUENT] Itself.</returns>
        public BadRequestException AddNull(string parameter)
        {
            Parameters[parameter] = $"Parameter {parameter} should not be null.";
            return this;
        }

        /// <summary>
        /// Adds a parameter that shouldn't be empty but is.
        /// </summary>
        /// <param name="parameter">The name of the parameter.</param>
        /// <returns>[FLUENT] Itself.</returns>
        public BadRequestException AddEmpty(string parameter)
        {
            Parameters[parameter] = $"Parameter {parameter} should not be empty.";
            return this;
        }

        /// <summary>
        /// Adds a parameter that is in a bad format.
        /// </summary>
        /// <param name="parameter">The name of the parameter.</param>
        /// <returns>[FLUENT] Itself.</returns>
        public BadRequestException AddBadFormat(string parameter)
        {
            Parameters[parameter] = $"Parameter {parameter} is in an incorrect format.";
            return this;
        }

        /// <summary>
        /// Adds a parameter that is incorrect.
        ///     This is the generic Add method, if any of the methods don't fit the scenario in use.
        /// </summary>
        /// <param name="parameter">The name of the parameter.</param>
        /// <param name="errorMessage">The error message to display for the parameter.</param>
        /// <returns>[FLUENT] Itself.</returns>
        public BadRequestException Add(string parameter, string errorMessage)
        {
            Parameters[parameter] = errorMessage;
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException" /> class with serialized data.</summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="StreamingContext" /> that holds the contextual information about the source or destination.</param>
        public BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            info.AddValue(nameof(Parameters), Parameters);
        }

        /// <summary>
        /// Converts this <see cref="BadRequestException"/> into a <see cref="BadRequestErrorResponse"/>.
        /// </summary>
        /// <returns>The converted <see cref="BadRequestErrorResponse"/>.</returns>
        internal BadRequestErrorResponse ToResponse()
        {
            return new BadRequestErrorResponse
            {
                Message = Message,
#if DEBUG
                StackTrace = StackTrace,
#endif
                Parameters = Parameters.Select(
                    p =>
                        new BadRequestParameter
                        {
                            Name = p.Key,
                            Description = p.Value
                        })
            };
        }
    }

    /// <summary>
    /// Contains utility extensions to help manipulate the throw of <see cref="BadRequestException"/>.
    /// </summary>
    public static class BadRequestThrowIf
    {
        /// <summary>
        /// Throws if any of the lambda parameters is Null or Whitespace.
        ///     It will agreegate all errors in the same <see cref="BadRequestException"/>.
        /// </summary>
        /// <param name="expressions">The list of lambdas with parameters.</param>
        public static void NullOrWhiteSpace(params Expression<Func<string>>[] expressions)
        {
            InternalAction(string.IsNullOrWhiteSpace, (x, s) => x.AddEmpty(s),  expressions);
        }

        /// <summary>
        /// Throws if any of the lambda parameters fails the required <paramref name="condition"/>.
        ///     It will agreegate all errors in the same <see cref="BadRequestException"/>.
        /// </summary>
        /// <param name="condition">The condition <see cref="Func{String, Bool}"/> that is required to pass.</param>
        /// <param name="exceptionExpression">The lamdba used to add the parameter to the <see cref="BadRequestException"/>.</param>
        /// <param name="expressions">The list of lambdas with parameters.</param>
        public static void InternalAction([NotNull]Func<string, bool> condition, Expression<Action<BadRequestException, string>> exceptionExpression, params Expression<Func<string>>[] expressions)
        {
            BadRequestException exception = null;
            foreach (var expression in expressions)
            {
                if (!condition(expression.Compile().Invoke())) continue;

                if (exception == null)
                {
                    exception = new BadRequestException(new StackFrame(1).GetMethod().Name);
                }

                exceptionExpression.Compile().Invoke(exception, GetMemberName(expression));
            }

            if (exception != null)
                throw exception;
        }

        /// <summary>
        /// Get's the member type from a given <see cref="Expression{T}"/>.
        ///     If it's a property member, then it finds the Declaring Type of prepends to the member name.
        /// </summary>
        /// <typeparam name="T">The member type of the expression.</typeparam>
        /// <param name="expression">The expression that we're getting the member name for.</param>
        /// <returns></returns>
        [NotNull]
        internal static string GetMemberName<T>([NotNull]Expression<Func<T>> expression)
        {
            var expressionBody = (MemberExpression)expression.Body;

            return expressionBody.Member.MemberType == MemberTypes.Property
                ? $"{expressionBody.Member.DeclaringType?.Name}.{expressionBody.Member.Name}"
                : expressionBody.Member.Name;
        }
    }
}

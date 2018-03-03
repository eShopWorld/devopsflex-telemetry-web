using System;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// exception for correlation namespace 
    /// </summary>
    public sealed class CorrelationVectorException: Exception
    {
        /// <summary>
        /// constructor with message
        /// </summary>
        /// <param name="msg">message</param>
        public CorrelationVectorException(string msg):base(msg)
        {
            
        }
    }
}

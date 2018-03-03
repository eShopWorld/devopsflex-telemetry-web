using System;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// exception for correlation namespace
    /// </summary>
    public sealed class CorrelationVectorException: Exception
    {
        public CorrelationVectorException(string msg):base(msg)
        {
            
        }
    }
}

using System;
using System.Text;

namespace Eshopworld.Web.Correlation
{
    /// <summary>
    /// correlation vector encapsulates string that identifies individual requests across services
    /// 
    /// the structure is as follows
    /// 
    /// {Guid}.{id}.{id}
    /// 
    /// each calling level augments the vector by adding a new identifier level
    /// </summary>
    public sealed class CorrelationVector
    {
        private uint _requestCount = 1;
        private readonly string _externalVector;

        public const string CorrelationVectorHeaderName = "X-Correlation-ID";

        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="externalVector">external vector as received</param>
        internal CorrelationVector(string externalVector)
        {
            _externalVector = externalVector;
        }       

        /// <summary>
        /// increment the count for subsequent request
        /// </summary>
        public void Increase()
        {
            _requestCount++;
        }

        /// <summary>
        /// serialize complete vector to string as Base64 UTF8 encoded string
        /// </summary>
        /// <returns>Base64 UTF8 encoded correlation string</returns>
        public override string ToString()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_externalVector}.{_requestCount}"));
        }
    }
}

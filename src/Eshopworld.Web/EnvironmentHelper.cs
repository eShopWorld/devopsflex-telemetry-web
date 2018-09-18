using System;

namespace Eshopworld.Web
{
    public class EnvironmentHelper
    {
        /// <summary>
        /// Determines if you are running in Service Fabric or not
        /// </summary>
        public static bool IsInFabric => Environment.GetEnvironmentVariable("Fabric_ApplicationName") != null;
    }
}

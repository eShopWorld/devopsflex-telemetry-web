using System;

namespace Eshopworld.Web
{
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Determines if you are running in Service Fabric or not
        /// </summary>
        public static bool IsInFabric =>  !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Fabric_ApplicationName"));
    }
}

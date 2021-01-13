using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Fabric.Description;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Eshopworld.Web
{
    [ExcludeFromCodeCoverage]
    public static class EswSslExtentions
    {
        /// <summary>
        /// Configures Kestrel to open listener for HTTP or HTTPS connection.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        /// <param name="port">The listener's port.</param>
        /// <param name="isHttps">Specifies whether the listener should handle HTTPS connections.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder UseEswSsl(this IWebHostBuilder builder, int port, bool isHttps)
        {
            return builder.UseEswSsl(new[] { (port, isHttps) });
        }

        /// <summary>
        /// Configures Kestrel to open listeners for all input http/https endpoints declared in the service manifest.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        /// <param name="listener">The listener used to access the endpoints configuration.</param>
        /// <param name="enableIpv6Binding">Switch to enable IPV6 binding - which is needed for the Service Fabric Proxy and Kestrel SSL binding to the app</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder UseEswSsl(this IWebHostBuilder builder, AspNetCoreCommunicationListener listener, bool enableIpv6Binding = false)
        {
            var ports = from endpoint in listener.ServiceContext.CodePackageActivationContext.GetEndpoints()
                        where endpoint.EndpointType == EndpointType.Input
                        where endpoint.Protocol == EndpointProtocol.Http || endpoint.Protocol == EndpointProtocol.Https
                        select (endpoint.Port, endpoint.Protocol == EndpointProtocol.Https);
            return builder.UseEswSsl(ports, enableIpv6Binding);
        }

        /// <summary>
        /// Configures Kestrel to open listeners for specified ports and handling HTTP or HTTPS protocol.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        /// <param name="endpoints">The enumeration of endpoints each defined by the port number and handled protocol type.</param>
        /// <param name="enableIpv6Binding">Switch to enable IPV6 binding - which is needed for the Service Fabric Proxy and Kestrel SSL binding to the app</param>
        /// <returns>The web builder.</returns>
        public static IWebHostBuilder UseEswSsl(this IWebHostBuilder builder, IEnumerable<(int port, bool isHttps)> endpoints, bool enableIpv6Binding = false)
        {
            var kestrelConfigurator=new KestrelConfigurator(endpoints,enableIpv6Binding);
            return builder.ConfigureKestrel(kestrelConfigurator.Configure);
        }

    }
}

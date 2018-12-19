using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Eshopworld.Core;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Eshopworld.Web
{
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
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder UseEswSsl(this IWebHostBuilder builder, AspNetCoreCommunicationListener listener)
        {
            var ports = from endpoint in listener.ServiceContext.CodePackageActivationContext.GetEndpoints()
                        where endpoint.EndpointType == EndpointType.Input
                        where endpoint.Protocol == EndpointProtocol.Http || endpoint.Protocol == EndpointProtocol.Https
                        select (endpoint.Port, endpoint.Protocol == EndpointProtocol.Https);
            return builder.UseEswSsl(ports);
        }

        /// <summary>
        /// Configures Kestrel to open listeners for specified ports and handling HTTP or HTTPS protocol.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        /// <param name="endpoints">The enumeration of endpoints each defined by the port number and handled protocol type.</param>
        /// <returns>The web builder.</returns>
        public static IWebHostBuilder UseEswSsl(this IWebHostBuilder builder, IEnumerable<(int port, bool isHttps)> endpoints)
        {
            return builder.ConfigureKestrel((context, options) =>
            {
                X509Certificate2 cert = null;
                // TODO: this is a simple solution to solve a temporary problem. Only the first endpoint is visible in SF Explorer. Better solutions are too complicated for our case.
                foreach (var (port, isHttps) in endpoints)
                {
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        if (isHttps)
                        {
                            try
                            {
                                if (cert == null)
                                    cert = GetCertificate(context.HostingEnvironment.EnvironmentName);
                                listenOptions.UseHttps(cert);
                                listenOptions.NoDelay = true;
                            }
                            catch (Exception ex)
                            {
                                BigBrother.Write(ex.ToExceptionEvent());
                                // TODO: can we do anything else than reverting to HTTP?
                            }
                        }
                    });
                }
            });
        }

        private static X509Certificate2 GetCertificate(string environmentName)
        {
            if (!Enum.TryParse<DeploymentEnvironment>(environmentName, true, out var environment))
            {
                environment = DeploymentEnvironment.Development;   // TODO: other default or exception? BB?
            }
            return GetCertificate(environment);
        }

        private static X509Certificate2 GetCertificate(DeploymentEnvironment environment)
        {
            var subject = GetCertSubjectName(environment);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, $"CN={subject}, OU=Domain Control Validated", true);

                if (certCollection.Count == 0)
                {
                    // TODO: another temporary attempt to find the cert
                    certCollection = store.Certificates.Find(X509FindType.FindBySubjectName, subject, false);
                }

                if (certCollection.Count == 0)
                {
                    throw new Exception($"The certificate for {subject} has not been found.");
                }

                return certCollection[0];
            }
            finally
            {
                store.Close();
            }
        }

        private static string GetCertSubjectName(DeploymentEnvironment environment)
        {
            switch (environment)
            {
                case DeploymentEnvironment.CI:
                    return "*.ci.eshopworld.net";
                case DeploymentEnvironment.Sand:
                    return "*.sandbox.eshopworld.com";
                case DeploymentEnvironment.Test:
                    return "*.test.eshopworld.net";
                case DeploymentEnvironment.Prep:
                    return "*.preprod.eshopworld.net";
                case DeploymentEnvironment.Prod:
                    return "*.production.eshopworld.com";
                case DeploymentEnvironment.Development:
                    return "localhost";
                default:
                    throw new ArgumentOutOfRangeException(nameof(environment), environment, $"The environment {environment} is not recognized");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Eshopworld.Core;
using Eshopworld.DevOps;
using Eshopworld.Telemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Eshopworld.Web
{
    internal class KestrelConfigurator
    {
        
        /// <summary>
        /// The ID of an certificate extension field which descibes the propose for which the public key may be used.
        /// </summary>
        private const string EnhancedKeyUsageOid = "2.5.29.37";

        /// <summary>
        /// The ID of the public key usage which allows using the certificate for the TLS authentication of a server.
        /// </summary>
        private const string ServerAuthenticationAllowedKeyUsage = "1.3.6.1.5.5.7.3.1";

        private readonly IEnumerable<(int port, bool isHttps)> _endpoints;
        private readonly bool _enableIpv6Binding;



        public KestrelConfigurator(IEnumerable<(int port, bool isHttps)> endpoints, bool enableIpv6Binding)
        {
            _endpoints = endpoints;
            _enableIpv6Binding = enableIpv6Binding;
        }

        public void Configure(WebHostBuilderContext context, KestrelServerOptions options)
        {
            X509Certificate2 cert = null;
            // TODO: this is a simple solution to solve a temporary problem. Only the first endpoint is visible in SF Explorer. Better solutions are too complicated for our case.
            foreach (var (port, isHttps) in _endpoints)
            {
                options.Listen(_enableIpv6Binding ? IPAddress.IPv6Any : IPAddress.Any, port, listenOptions =>
                {
                    if (isHttps)
                    {
                        try
                        {
                            if (cert == null)
                                cert = GetCertificate(context.HostingEnvironment.EnvironmentName);
                            listenOptions.UseHttps(cert);
                        }
                        catch (Exception ex)
                        {
                            BigBrother.Write(ex.ToExceptionEvent());
                            // TODO: can we do anything else than reverting to HTTP?
                        }
                    }
                });
            }
        }

        internal virtual X509Certificate2 GetCertificate(string environmentName)
        {
            if (!Enum.TryParse<DeploymentEnvironment>(environmentName, true, out var environment))
            {
                environment = DeploymentEnvironment.Development;   // TODO: other default or exception? BB?
            }
            return GetCertificate(environment);
        }

        internal static X509Certificate2 GetCertificate(DeploymentEnvironment environment, Func<X509Certificate2,bool> isSlCertificatePredicate=null)
        {
            isSlCertificatePredicate ??= IsSslCertificate;
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

                    for (int i = 0; i < certCollection.Count; i++)
                    {
                        if (!isSlCertificatePredicate(certCollection[i]))
                        {
                            certCollection.RemoveAt(i);
                            i--;
                        }
                        else if (certCollection[i].FriendlyName == "ASP.NET Core HTTPS development certificate")
                        {
                            return certCollection[i];
                        }
                    }
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


        /// <summary>
        /// Checks whether the certificate can be used to enable SSL traffic to the local server
        /// </summary>
        /// <param name="cert">Certificate to check.</param>
        /// <returns>True if the certificate can be used for SSL decoding, false otherwise.</returns>
        internal static bool IsSslCertificate(X509Certificate2 cert)
        {
            // based on https://stackoverflow.com/questions/51322480/x509-certificate-intended-purpose-and-ssl
            if (!cert.HasPrivateKey)
            {
                return false;
            }

            foreach (var extension in cert.Extensions)
            {
                if (extension.Oid.Value == EnhancedKeyUsageOid)
                {
                    var eku = (X509EnhancedKeyUsageExtension)extension;
                    foreach (var oid in eku.EnhancedKeyUsages)
                    {
                        if (oid.Value == ServerAuthenticationAllowedKeyUsage)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}

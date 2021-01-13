using System;
using System.Security.Cryptography.X509Certificates;
using Eshopworld.DevOps;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eshopworld.Web.Tests
{
    internal class TestHostingEnvironment :  IWebHostEnvironment
    {
        public string EnvironmentName { get; set; }
        public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    public class KestrelConfiguratorTests
    {
        [Fact]
        public void Configure_WhenSSlEndpoints_ShouldCreateCertificate()
        {
            //Arrange
            var endPoints = new (int port, bool isHttps)[] { (443, true) };
            var sut = new Mock<KestrelConfigurator>(endPoints, true);
          
            var webHostBuilderContext = new WebHostBuilderContext() { HostingEnvironment = new TestHostingEnvironment{EnvironmentName = "foo"} };
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(new Mock<ILoggerFactory>().Object);
            var cert = new X509Certificate2();
            sut.Setup(m => m.GetCertificate(webHostBuilderContext.HostingEnvironment.EnvironmentName)).Returns(cert).Verifiable();

            //Act
            sut.Object.Configure(webHostBuilderContext, new KestrelServerOptions(){ ApplicationServices = serviceProvider.Object});

            //Assert
            sut.Verify();

        }

        [Fact]
        public void Configure_WhenNoSSlEndpoints_ShouldNotCreateCertificate()
        {
            //Arrange
            var endPoints = new (int port, bool isHttps)[] {(80, false)};
            var sut=new Mock<KestrelConfigurator>(endPoints,false);
           
            //Act
            sut.Object.Configure(It.IsAny<WebHostBuilderContext>(),new KestrelServerOptions());

            //Assert
            sut.Verify(m => m.GetCertificate(It.IsAny<string>()),Times.Never);

        }

        [Fact]
        public void GetCertificate_WhenValidCertificate_ShouldSucceed()
        {
            

            //Act
            var cert=KestrelConfigurator.GetCertificate(DeploymentEnvironment.Development,(cert)=>true);

            //Assert
            cert.Should().NotBe(null);
        }

        [Fact]
        public void GetCertificate_WhenNotValidCertificate_ShouldThrow()
        {
            //Act
            Action act = () => KestrelConfigurator.GetCertificate(DeploymentEnvironment.Development, (cert) => false);

            //Assert
            act.Should().Throw<Exception>().Where(e => e.Message.StartsWith("The certificate for"));
        }

        [Fact]
        public void IsSslCertificate_WhenValidCertificate_ShouldReturnTrue()
        {
            //Arrange
            var cert = GetValidCertificate();

            //Act
            var result=KestrelConfigurator.IsSslCertificate(cert);

            //Assert
            result.Should().BeTrue();
        }


        public static X509Certificate2 GetValidCertificate()
        {
            string certificateString = "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";
            var privateKeyBytes = Convert.FromBase64String(certificateString);
            var pfxPassword = "Password";
            return new X509Certificate2(privateKeyBytes, pfxPassword);
        }

    }
}

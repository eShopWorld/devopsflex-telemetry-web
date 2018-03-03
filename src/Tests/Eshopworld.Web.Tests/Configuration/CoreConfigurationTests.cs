using DevOpsFlex.Tests.Core;
using FluentAssertions;
using System;
using System.IO;
using System.Reflection;
using Eshopworld.Web.Configuration;
using Xunit;

namespace Eshopworld.Web.Tests.Configuration
{
    /// <summary>
    /// tests for <see cref="CoreConfiguration"/>
    /// </summary>
    public class CoreConfigurationTests : IClassFixture<TestsFixture>
    {
        [Fact, IsIntegration]
        public void Test_ReadFromCoreAppSettings()
        {
            //arrange
            var sut = CoreConfiguration.Build(AssemblyDirectory);
            //assert
            sut["KeyRootAppSettings"].ShouldBeEquivalentTo("AppSettingsValue");
        }


        [Fact, IsIntegration]
        public void Test_ReadFromTestAppSettings()

        {

            //arrange
            var sut = CoreConfiguration.Build(AssemblyDirectory, useTest: true);
            //assert
            sut["KeyTestAppSettings"].ShouldBeEquivalentTo("TestAppSettingsValue");
        }



        [Fact, IsIntegration]
        public void Test_ReadFromEnvironmentalAppSettings()

        {
            //arrange
            var sut = CoreConfiguration.Build(AssemblyDirectory, "ENV1");
            //assert
            sut["KeyENV1AppSettings"].ShouldBeEquivalentTo("ENV1AppSettingsValue");
        }


        [Fact, IsIntegration]
        public void Test_ReadFromEnvironmentalVariable()
        {
            //arrange
            var sut = CoreConfiguration.Build(AssemblyDirectory);
            //assert
            sut["PATH"].Should().NotBeNullOrEmpty();
        }


        [Fact, IsIntegration]
        public void Test_ReadFromKeyVault()
        {
            //arrange
            var sut = CoreConfiguration.Build(AssemblyDirectory);
            //assert
            sut["keyVaultItem"].ShouldBeEquivalentTo("keyVaultItemValue");
        }



        /// <summary>
        /// the test runner will shadow copy the assemblies so to resolve the config files this is needed
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}

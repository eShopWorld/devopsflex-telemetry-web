using System;
using System.IO;

namespace Eshopworld.Web.Tests.Configuration
{
    public class TestsFixture : IDisposable
    {
        public TestsFixture()
        {
            var secret = Environment.GetEnvironmentVariable("DEVOPSFLEX-TESTS-KVSECRET",EnvironmentVariableTarget.Machine);
            File.WriteAllText(Path.Combine(CoreConfigurationTests.AssemblyDirectory, "appsettings.KV.json"), $"{{\"KeyVaultName\": \"devopsflex-tests\",  \"KeyVaultClientId\": \"848c5ccc-8dad-4f0a-885d-1c50ab17f611\",\"KeyVaultClientSecret\": \"{secret}\"}}");
        }

        public void Dispose()
        {
            File.Delete(Path.Combine(Path.GetDirectoryName(CoreConfigurationTests.AssemblyDirectory), "appsettings.KV.json"));
        }

    }
}

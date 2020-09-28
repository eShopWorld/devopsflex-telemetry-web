using System;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class CustomSerializationBinderTests
    {
        private readonly CustomSerializationBinder _customSerializationBinder = new CustomSerializationBinder();

        [Fact, IsUnit]
        public void BindToName_ForTypeProvided_ProvidesAssemblyAndName()
        {
            // Act
            _customSerializationBinder.BindToName(typeof(TypeForTests), out var assembly, out var name);

            // Assert
            using var _ = new AssertionScope();
            assembly.Should().Be("Eshopworld.Web.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            name.Should().Be("Eshopworld.Web.Tests.TypeForTests");
        }

        [Fact, IsUnit]
        public void BindToName_ForTypeDescriptionProvided_ReturnsType()
        {
            // Act
            var resultType = _customSerializationBinder.BindToType("Eshopworld.Web.Tests", "Eshopworld.Web.Tests.TypeForTests");

            // Assert
            resultType.Should().Be<TypeForTests>();
        }

        [Fact, IsUnit]
        public void BindToName_ForTypeDescriptionProvidedWithServiceName_ThrowsExceptionWheAssemblyUnavailable()
        {
            // Act
            Action testAction = () => _customSerializationBinder.BindToType("ABC_.service.mtabc", "Eshopworld.Web.Tests.TypeForTests");

            // Assert
            testAction.Should().ThrowExactly<JsonSerializationException>()
                .WithMessage("Could not load assembly 'ABC_.service.mtabc'.");
        }
    }

    internal class TypeForTests
    {
    }
}
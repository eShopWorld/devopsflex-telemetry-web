using System;
using System.Linq;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class ServiceRemotingJsonSerializationProviderTests
    {
        private readonly ServiceRemotingJsonSerializationProvider _provider = new ServiceRemotingJsonSerializationProvider();

        [Fact, IsUnit]
        public void CreateMessageBodyFactory_AlwaysCreatesFactory()
        {
            // Act
            var result = _provider.CreateMessageBodyFactory();

            // Assert
            result.Should().BeOfType<JsonMessageFactory>();
        }

        [Fact, IsUnit]
        public void CreateRequestMessageSerializer_AlwaysCreatesSerializer()
        {
            // Act
            var result = _provider.CreateRequestMessageSerializer(typeof(TypeForTests), Enumerable.Empty<Type>());

            // Assert
            result.Should().BeOfType<ServiceRemotingRequestJsonMessageBodySerializer>();
        }

        [Fact, IsUnit]
        public void CreateResponseMessageSerializer_AlwaysCreatesSerializer()
        {
            // Act
            var result = _provider.CreateResponseMessageSerializer(typeof(TypeForTests), Enumerable.Empty<Type>());

            // Assert
            result.Should().BeOfType<ServiceRemotingResponseJsonMessageBodySerializer>();
        }
    }
}
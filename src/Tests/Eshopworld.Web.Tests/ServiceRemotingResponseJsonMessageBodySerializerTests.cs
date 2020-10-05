using System.IO;
using System.Text;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using Moq;
using Xunit;

namespace Eshopworld.Web.Tests
{
    public class ServiceRemotingResponseJsonMessageBodySerializerTests
    {
        private readonly ServiceRemotingResponseJsonMessageBodySerializer _serializer = new ServiceRemotingResponseJsonMessageBodySerializer();

        [Fact, IsUnit]
        public void Serialize_ForInputJsonBody_GeneratesSingleBuffer()
        {
            // Act
            var result = _serializer.Serialize(new JsonBody(new { Abc = "dummy-value" }));

            // Assert
            result.GetSendBuffers().Should().HaveCount(1)
                .And.Should().NotBeNull();
        }

        [Fact, IsUnit]
        public void Serialize_ForInputJsonBodyNull_ReturnsNull()
        {
            // Act
            var result = _serializer.Serialize(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact, IsUnit]
        public void Deserialize_WhenBodyWithValueProvided_DeserializedProperly()
        {
            // Arrange
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes("{ Value: \"dummy-value\" }"));
            var mock = new Mock<IIncomingMessageBody>(MockBehavior.Strict);
            mock.Setup(m => m.GetReceivedBuffer()).Returns(jsonStream);

            // Act
            var result = _serializer.Deserialize(mock.Object);

            // Assert
            result.Should().BeOfType<JsonBody>().Which.Should().BeEquivalentTo(new { Value = "dummy-value" });
        }
    }
}
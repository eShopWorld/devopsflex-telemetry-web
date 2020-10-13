using System;

namespace Eshopworld.Web.Tests
{
    using System.IO;
    using System.Linq;
    using Eshopworld.Tests.Core;
    using System.Runtime.Serialization.Formatters.Binary;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Xunit;


    // ReSharper disable once CheckNamespace
    public class BadRequestExceptionTest
    {
        public class ToResponse
        {
            [Fact, IsLayer0]
            public void Test_AllParameterTypes_ConvertToResponse()
            {
                const string nullParam = "my null param";
                const string badFormatParam = "my bad format param";
                const string genericBadParam = "my generic bad param";
                const string genericBadParamMessage = "this is bad!";

                var ex = new BadRequestException().AddNull(nullParam)
                    .AddBadFormat(badFormatParam)
                    .Add(genericBadParam, genericBadParamMessage);

                var result = ex.ToResponse();

                result.Parameters.Should().HaveCount(3);

                result.Parameters.Select(p => p.Name).Should().Contain(nullParam);
                result.Parameters.Select(p => p.Name).Should().Contain(badFormatParam);
                result.Parameters.Select(p => p.Name).Should().Contain(genericBadParam);
            }
        }

        public class NullOrWhiteSpace
        {
            [Fact, IsLayer0]
            public void Test_Throws_WithDoubleParamPocoFailure()
            {
                var foo = new TestRequest();
                Action throwIfNullOrWhitespace = () =>
                    BadRequestThrowIf.NullOrWhiteSpace(() => foo.Param1, () => foo.Param2);

                throwIfNullOrWhitespace.Should().Throw<BadRequestException>()
                    .And.Parameters.Should().HaveCount(2)
                    .And.ContainKey($"{nameof(TestRequest)}.{nameof(TestRequest.Param1)}")
                    .And.ContainKey($"{nameof(TestRequest)}.{nameof(TestRequest.Param2)}");

            }
        }

        public class ThrowIfInvalid
        {
            [Fact, IsLayer0]
            public void Test_ModelState_With1Error()
            {
                const string errorKey = "my error key";
                const string errorMsg = "my error message";

                var ms = new ModelStateDictionary();
                ms.AddModelError(errorKey, errorMsg);

                Action throwIfInvalid = () => ms.ThrowIfInvalid();

                throwIfInvalid.Should().Throw<BadRequestException>()
                    .And.Parameters.Should().HaveCount(1)
                    .And.Contain(errorKey, errorMsg);
            }

            [Fact, IsLayer0]
            public void Test_ModelState_With1Exception()
            {
                const string errorKey = "my error key";
                const string errorMsg = "my error message";

                var ms = new ModelStateDictionary();
                var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object));
                ms.AddModelError(errorKey, new Exception(errorMsg), metadata);

                Action throwIfInvalid = () => ms.ThrowIfInvalid();

                throwIfInvalid.Should().Throw<BadRequestException>()
                    .And.Parameters.Should().HaveCount(1)
                    .And.Contain(errorKey, errorMsg);
            }
        }

        public class SerializationTests
        {
            [Fact, IsLayer0]
            public void ISerializationImpl_Success()
            {
                var sut = new BadRequestException();
                sut.Add("test", "msg");

                var stream = new MemoryStream();
                var serializer = new BinaryFormatter();

                //serialize
                serializer.Serialize(stream, sut);
                //deserialize
                stream.Position = 0;
                var ds = serializer.Deserialize(stream) as BadRequestException;
                //assert
                Assert.NotNull(ds);
                Assert.Contains(ds.Parameters, i => i.Key == "test" && i.Value == "msg");

            }
        }
    }

    public class TestRequest
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
    }
}

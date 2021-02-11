using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Eshopworld.Web.Tests
{
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Eshopworld.Tests.Core;
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

                var ex = new BadRequestException()
                    .AddNull(nullParam)
                    .AddBadFormat(badFormatParam)
                    .Add(genericBadParam, genericBadParamMessage);

                var result = ex.ToResponse();

                var expectedParams = new List<BadRequestParameter>
                {
                    new BadRequestParameter { Name=nullParam, Description = $"Parameter {nullParam} should not be null."},
                    new BadRequestParameter { Name=badFormatParam, Description = $"Parameter {badFormatParam} is in an incorrect format."},
                    new BadRequestParameter { Name=genericBadParam, Description = genericBadParamMessage}
                };

                result.Parameters.Should().BeEquivalentTo(expectedParams);

                result.Caller.Should().BeNullOrEmpty();
            }
        }

        public class NullOrWhiteSpace
        {
            [Fact, IsLayer0]
            public void Test_Throws_WithDoubleParamPocoFailure()
            {
                var foo = new TestRequest();
                var blewUp = false;

                try
                {
                    BadRequestThrowIf.NullOrWhiteSpace(() => foo.Param1, () => foo.Param2);
                }
                catch (BadRequestException e)
                {
                    blewUp = true;
                    e.Parameters.Should().HaveCount(2);
                    e.Parameters.Should().ContainKey($"{nameof(TestRequest)}.{nameof(TestRequest.Param1)}");
                    e.Parameters.Should().ContainKey($"{nameof(TestRequest)}.{nameof(TestRequest.Param2)}");
                }

                blewUp.Should().BeTrue();
            }
        }

        public class ThrowIfInvalid
        {
            [Fact, IsLayer0]
            public void Test_ModelState_WithNoError()
            {
                var ms = new ModelStateDictionary();

                ms.Invoking(x => x.ThrowIfInvalid()).Should().NotThrow();
            }

            [Fact, IsLayer0]
            public void Test_ModelState_With1Error()
            {
                const string errorKey = "my error key";
                const string errorMsg = "my error message";

                var threwBadRequest = false;
                var ms = new ModelStateDictionary();
                ms.AddModelError(errorKey, errorMsg);

                try
                {
                    ms.ThrowIfInvalid();
                }
                catch (BadRequestException e)
                {
                    threwBadRequest = true;
                    e.Parameters.Should().HaveCount(1);
                    e.Parameters.FirstOrDefault().Key.Should().Be(errorKey);
                    e.Parameters.FirstOrDefault().Value.Should().Be(errorMsg);
                }

                threwBadRequest.Should().BeTrue();
            }

            [Fact, IsLayer0]
            public void Test_ModelState_With1Exception()
            {
                const string errorKey = "my error key";
                const string errorMsg = "my error message";

                var threwBadRequest = false;
                var ms = new ModelStateDictionary();
                ms.AddModelError(errorKey, new Exception(errorMsg), new TestModelMetadata(ModelMetadataIdentity.ForType(typeof(string))));

                try
                {
                    ms.ThrowIfInvalid();
                }
                catch (BadRequestException e)
                {
                    threwBadRequest = true;
                    e.Parameters.Should().HaveCount(1);
                    e.Parameters.FirstOrDefault().Key.Should().Be(errorKey);
                    e.Parameters.FirstOrDefault().Value.Should().Be(errorMsg);
                }

                threwBadRequest.Should().BeTrue();
            }

            private class TestModelMetadata : ModelMetadata
            {
                public TestModelMetadata(ModelMetadataIdentity identity) : base(identity)
                {
                }

                public override IReadOnlyDictionary<object, object> AdditionalValues { get; }
                public override string BinderModelName { get; }
                public override Type BinderType { get; }
                public override BindingSource BindingSource { get; }
                public override bool ConvertEmptyStringToNull { get; }
                public override string DataTypeName { get; }
                public override string Description { get; }
                public override string DisplayFormatString { get; }
                public override string DisplayName { get; }
                public override string EditFormatString { get; }
                public override ModelMetadata ElementMetadata { get; }
                public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues { get; }
                public override IReadOnlyDictionary<string, string> EnumNamesAndValues { get; }
                public override bool HasNonDefaultEditFormat { get; }
                public override bool HideSurroundingHtml { get; }
                public override bool HtmlEncode { get; }
                public override bool IsBindingAllowed { get; }
                public override bool IsBindingRequired { get; }
                public override bool IsEnum { get; }
                public override bool IsFlagsEnum { get; }
                public override bool IsReadOnly { get; }
                public override bool IsRequired { get; }
                public override ModelBindingMessageProvider ModelBindingMessageProvider { get; }
                public override string NullDisplayText { get; }
                public override int Order { get; }
                public override string Placeholder { get; }
                public override ModelPropertyCollection Properties { get; }
                public override IPropertyFilterProvider PropertyFilterProvider { get; }
                public override Func<object, object> PropertyGetter { get; }
                public override Action<object, object> PropertySetter { get; }
                public override bool ShowForDisplay { get; }
                public override bool ShowForEdit { get; }
                public override string SimpleDisplayProperty { get; }
                public override string TemplateHint { get; }
                public override bool ValidateChildren { get; }
                public override IReadOnlyList<object> ValidatorMetadata { get; }
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

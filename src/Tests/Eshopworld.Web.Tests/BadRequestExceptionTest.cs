namespace Eshopworld.Web.Tests
{
    using System.Linq;
    using DevOpsFlex.Tests.Core;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Xunit;

// ReSharper disable once CheckNamespace
    public class BadRequestExceptionTest
    {
        public class ToResponse
        {
            [Fact, IsUnit]
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
            [Fact, IsUnit]
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
            [Fact, IsUnit]
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
        }
    }

    public class TestRequest
    {
        public string Param1 { get; set; }

        public string Param2 { get; set; }
    }
}
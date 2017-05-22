using System.Linq;
using DevOpsFlex.Telemetry.Web;
using DevOpsFlex.Tests.Core;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
public class BadRequestExceptionTest
{
    public class ToResponse
    {
        [Fact, IsUnit]
        public void Test_AllParameterTypes_ConvertToResponse()
        {
            const string method = "my method";
            const string nullParam = "my null param";
            const string badFormatParam = "my bad format param";
            const string genericBadParam = "my generic bad param";
            const string genericBadParamMessage = "this is bad!";

            var ex = new BadRequestException(method).AddNull(nullParam)
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
}

public class TestRequest
{
    public string Param1 { get; set; }

    public string Param2 { get; set; }
}

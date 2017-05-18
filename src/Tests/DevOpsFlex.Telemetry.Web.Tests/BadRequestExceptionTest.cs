using DevOpsFlex.Telemetry.Web;
using DevOpsFlex.Tests.Core;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
public class BadRequestExceptionTest
{
    [Fact, IsUnit]
    public void Test_ThrowIfNullOrWhiteSpace_Throws_WithDoubleParamPocoFailure()
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

public class TestRequest
{
    public string Param1 { get; set; }

    public string Param2 { get; set; }
}

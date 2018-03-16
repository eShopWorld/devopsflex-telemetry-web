# web

Contains ASPNET Core middleware and utility classes to facilitate bootstrapping [devops-telemetry](https://github.com/eShopWorld/devopsflex-telemetry) in ASPNET Core applications

### How do I bootstrap it?

In your Startup `Configure` method just do `UseBigBrotherExceptionHandler` at the start of the pipeline

```c#
public void Configure(IApplicationBuilder app)
{
    // This should be done at the start of the pipeline or it won't catch exceptions
    // that happen on the pipeline before this
    app.UseBigBrotherExceptionHandler();
}
```

The middleware class needs `IBigBrother` as a service dependency, so you need to make sure it's bootstrapped.

### What about configuration helpers?

The package contains a Telemetry POCO to facilitate binding to configuration sections

```c#
/// <summary>
/// Contains settings related to Telemetry.
/// </summary>
public class TelemetrySettings
{
    /// <summary>
    /// Gets and sets the main telemetry instrumentation key.
    /// </summary>
    public string InstrumentationKey { get; set; }

    /// <summary>
    /// Gets and sets the internal instrumentation key.
    /// </summary>
    public string InternalKey { get; set; }
}
```

That will bind to the `Telemetry` section of this JSON settings example

```json
{
    "Telemetry": {
        "InstrumentationKey": "SOME-KEY",
        "InternalKey": "ANOTHER-KEY"
    }
}
```

### BadRequestException

The middleware will convert `BadRequestException` into an HTTP400 and relevant JSON payload. It contains a set of extension methods that facilitate working with this Exception:

```c#
[HttpPost]
[SwaggerOperation("CreateBrowser")]
[ProducesResponseType(typeof(BrowserResponse), 200)]
public async Task<IActionResult> CreateAsync([FromBody] BrowserRequest newBrowser)
{
    BadRequestThrowIf.NullOrWhiteSpace(() => newBrowser.SomeParam, () => newBrowser.AnotherParam);

    // Do some stuff

    return Ok();
}
```
# web

Contains ASPNET Core middleware and utility classes to facilitate bootstrapping [devops-telemetry](https://github.com/eShopWorld/devopsflex-telemetry) in ASPNET Core applications


### Helpers

* Checks if the app is running in a Service Fabric or not:

```c#
EnvironmentHelper.IsInFabric()
```

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

### RequestTelemetryInitializer

The RequestTelemetryInitializer allows interception of a request to decorate your AI telemetry.
As Application Insights doesn't read request bodies, this telemetry initializer can contains functionality to help with reading the request body.
For example, this is beneficial in establishing correlation for requests where the data is contained within the request body.

To enable this functionality within your application, first create a custom class which will inherit from `Eshopworld.Web.Telemetry.RequestTelemetryInitializer`, and then register its type `services.AddSingleton<ITelemetryInitializer, RequestBodyTelemetryInitializer>();` in `ConfigureServices` in your `Startup.cs`.

#### Examples

In the example below, two routes are monitored, and elements of their bodies are interrogated and sent to AI as part of the Request Telemetry as a custom dimension. This will allow requests to be tracked / queries by the various data.
This is especially useful in webhook endpoints where there may be no identifiers in the request URI.

``` c#
internal class RequestBodyTelemetryInitializer : Eshopworld.Web.Telemetry.RequestTelemetryInitializer
{
    public RequestBodyTelemetryInitializer([NotNull] IHttpContextAccessor httpContextAccessor, [NotNull] IBigBrother bigBrother) : base(httpContextAccessor, bigBrother) { }

    protected override void HandleIncomingRequest(string path, RequestTelemetry requestTelemetry, HttpRequest request)
    {
        switch (path)
        {
            case "/api/v1/Foo":
                requestTelemetry.Properties["FooId"] = ExtractBodyDetail<FooDTO>(request, dto => dto?.Id.ToString());
                break;
            
            case "/api/v1/Bar":
                requestTelemetry.Properties["BarId"] = ExtractBodyDetail<BarDTO>(request, dto => dto?.Id.ToString());
                break;
        }
    }
}
```

All routes are monitored, and the AuthenticatedUserId property is set on the user context within the Request Telemetry, with the value obtained from the *client_id* claim obtained from the user's claim set.

Note, this must currently occur in `HandleCompletedRequest`, as the security context has not been established when `HandleIncomingRequest` is triggered.

``` c#
internal class SecurityResponseTelemetryInitializer : Eshopworld.Web.Telemetry.RequestTelemetryInitializer
{
    public SecurityResponseTelemetryInitializer([NotNull] IHttpContextAccessor httpContextAccessor, [NotNull] IBigBrother bigBrother) : base(httpContextAccessor, bigBrother) { }

    protected override void HandleCompletedRequest(string path, RequestTelemetry requestTelemetry, HttpRequest request)
    {
        requestTelemetry.Context.User.AuthenticatedUserId = (httpContextAccessor.HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity)?.Claims?.SingleOrDefault(c => c.Type == "client_id")?.Value;
    }
}
```
### Evolution Fallback Polly policy

Custom policy has been developed that is tailored specifically for V2 Evolution deployments. As the service is deployed, it can be routed to via ServiceFabric reverse proxy (if enabled), ELB or FrontDoor. These routes are hierarchical.
For performance reasons, the closest route is preferred as it is assumed it would be the fastest. If the route however fails, we want to move to higher level and retry as this may succeed. Ultimately the full flow envisioned is Reverse Proxy route-> ELB (Cluster level) -> FrontDoor route. FrontDoor route offers cross-regional failover.

This policy wraps timeout internally to provide meaningful fallback policy. Developer has full control over this timeout via configuration.

Configuration builder allows to 
 - set # of retries per level
 - retry timeout
 - wait time between retries

**Please note** that the policy timeout is per request. HttpClient timeout is an overall timeout across ALL calls. This is inline with Polly implementation - see https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#use-case-applying-timeouts. Make sure that overall timeout is sufficient given the timeout set for individual attempt and number of attempts at level.

**Also note** that if this policy is used in the context of autorest client, it is advised to remove the internal transient error handling built into autorest/rest client. This policy handles transient issues along with the fallback behaviour.

``` c#

//switch off default rest client transient error handling
 autorestClient.SetRetryPolicy(null);

```

This policy can be further combined with additional policies as per Polly design e.g. Circuit breaker.

Sample usage follows

``` c#

  var dns = new DnsLevel
    {
        Cluster = "https://cluster_route",
        Proxy = "https://reverseProxy_route",
        Global = "https://FrontDoor_route"
    };

    var configBuilder= new EvoFallbackPollyPolicyConfigurationBuilder();
    var config = configBuilder
        .SetRetriesPerLevel(5)
        .SetRetryTimeOut(TimeSpan.FromSeconds(10))
        .SetWaitTimeBetweenRetries(TimeSpan.FromMilliseconds(50))
        .Build();

    var eswRetryPolicy = EvoFallbackPollyPolicyBuilder.EswDnsRetryPolicy(config);

    var circuitBreaker = Policy<HttpResponseMessage>
        .HandleResult(resp=>!resp.IsSuccessStatusCode)
        .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1))
        .WrapAsync(eswRetryPolicy);

    var autorestClient =
        new SwaggerPetstore(
            new EvoFallbackPollyPolicyHttpHandler(circuitBreaker, dns)
            );

    autorestClient.HttpClient.Timeout = TimeSpan.FromMinutes(10);
    autorestClient.SetRetryPolicy(null);
	
var resp = await autorestClient.GetPetByIdWithHttpMessagesAsync(1);

```
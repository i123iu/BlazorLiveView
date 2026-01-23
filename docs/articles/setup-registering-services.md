# Setup: Registering Services

This is the second step of BlazorLiveView setup: **Registering the required services in `Program.cs`**.

## Service Registration

BlazorLiveView services need to be registered in your dependency injection container. Add the following code to `Program.cs` before building the application.

```csharp
using BlazorLiveView.Core.Extensions;
builder.AddLiveView();
```

### Configuration Options

The method `AddLiveView` also accepts optional configuration options:

```csharp
builder.AddLiveView(options =>
{
    // The URI path for the mirror endpoint (default: "/_mirror")
    options.MirrorUri = "/custom-mirror-path";

    // Whether to use a transparent overlay element on mirrors to prevent interaction
    options.UseScreenOverlay = true;
});
```

## Mapping the Mirror Endpoint

To be able to view the mirrored sessions, the _mirror endpoint_ must be mapped. Add the following code to `Program.cs` after building the application and before calling `app.Run()`.

```csharp
using BlazorLiveView.Core.Extensions;
app.MapLiveViewMirrorEndpoint();
```

Note that the previous code is **NOT SECURE** by default. Anyone could access the mirror endpoint if they know the circuit ID. To add authentication and authorization, see the next section.

### Adding Authorization

To secure the mirror endpoint, the method `MapLiveViewMirrorEndpoint` accepts a configuration action where you can configure the mirror endpoint.

```csharp
app.MapLiveViewMirrorEndpoint(mirrorEndpoint =>
{
    mirrorEndpoint.RequireAuthorization("PolicyName");
});
```

## Complete `Program.cs` Example

The complete `Program.cs` file may look like this:

```csharp
using BlazorLiveView.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Other services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register BlazorLiveView services
builder.AddLiveView();

var app = builder.Build();

// Other middleware
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map the mirror endpoint
app.MapLiveViewMirrorEndpoint();

app.Run();
```

## Next Steps

Continue with [Setup: Dashboard](setup-dashboard.md) to add the admin user interface.

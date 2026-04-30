# Setup: Registering Services

This is the second step of BlazorLiveView setup: **Registering the required services in `Program.cs`**.

## Service Registration

BlazorLiveView services need to be registered in your dependency injection container. Call the method `AddLiveView()` on `builder.Services` in your `Program.cs`:

```csharp
using BlazorLiveView.Core.Extensions;
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView();
```

The method `AddLiveView` also accepts optional configuration options:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView(options =>
    {
        // The URI path for the mirror endpoint (default: "/_mirror")
        options.MirrorUri = "/custom-mirror-path";

        // Whether to use a transparent overlay element on mirrors to prevent interaction
        options.UseScreenOverlay = true;
        
        // And more...
    });
```

## Mapping the Mirror Endpoint

To be able to view the mirrored sessions, the _mirror endpoint_ must be mapped. Call the method `MapLiveViewMirrorEndpoint()` on `WebApplication app` in `Program.cs` after building the application and before calling `app.Run()`:

```csharp
using BlazorLiveView.Core.Extensions;
app.MapLiveViewMirrorEndpoint();
```

Note that the previous code is **NOT SECURE** by default. Anyone could access the mirror endpoint if they know the circuit ID. To add authentication and authorization, the method `MapLiveViewMirrorEndpoint` accepts a configuration action where you can configure the mirror endpoint. For example:

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

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView();

var app = builder.Build();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapLiveViewMirrorEndpoint();

app.Run();
```

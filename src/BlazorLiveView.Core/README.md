# BlazorLiveView.Core

Core functionality for the BlazorLiveView library. See the main package: `BlazorLiveView`.

## Reference setup

Register necessary services in `Program.cs`:

```csharp
using BlazorLiveView.Core.Extensions;
WebApplicationBuilder builder;
builder.AddLiveViewBackend();
```

Map the mirror endpoint in `Program.cs`:

```csharp
using BlazorLiveView.Core.Extensions;
WebApplication app;
app.MapLiveViewMirrorEndpoint();
```

Alternatively, add authorization to the mirror endpoint:

```csharp
using BlazorLiveView.Core.Extensions;
WebApplication app;
app.MapLiveViewMirrorEndpoint(endpoint =>
{
    endpoint.RequireAuthorization("PolicyName");
});
```

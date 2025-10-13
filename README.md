# BlazorLiveView

BlazorLiveView is a real-time screen sharing and monitoring library for ASP.NET Blazor Server applications. It is similar to remote desktop tools like TeamViewer, but native to Blazor web applications.

## Reference setup

Register necessary services in `Program.cs`:

```csharp
using BlazorLiveView.Extensions;
WebApplicationBuilder builder;
builder.AddLiveViewBackendAndDashboard();
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

Include the `LiveViewDashboard` component to a Razor page:

```razor
@page "/liveview-dashboard"
@using BlazorLiveView.Dashboard.Components
<LiveViewDashboard />
```

## Features

- Real-time screen mirroring
- Admin dashboard showing all active connections
- Native performance using Blazor's internal rendering system
- Minimal modifications to existing Blazor components

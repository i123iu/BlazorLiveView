# BlazorLiveView [![Tests status](https://img.shields.io/github/actions/workflow/status/i123iu/BlazorLiveView/test.yml?branch=main)](https://github.com/i123iu/BlazorLiveView/actions)

BlazorLiveView is a real-time screen sharing library for ASP.NET Blazor Server applications. It can be useful for debugging and remote assistance. It is similar to remote desktop tools like TeamViewer, but native to Blazor web applications.

<img src="https://raw.githubusercontent.com/i123iu/BlazorLiveView/main/icon.png" alt="BlazorLiveView" width="128" />

## Features

- Real-time screen mirroring
- Admin dashboard showing all active connections
- Native performance using Blazor's internal rendering system
- Minimal modifications to existing Blazor components

## Reference setup

Create a new Blazor Web App project, select:
- "Interactive render mode" -> "Server"
- "Interactivity location" -> "Global"

Install the [NuGet package](https://www.nuget.org/packages/BlazorLiveView):

```
dotnet add package BlazorLiveView
```

Register necessary services in `Program.cs`:

```csharp
using BlazorLiveView.Extensions;
WebApplicationBuilder builder;
builder.AddLiveView();
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

Include the `LiveViewDashboard` component in a Razor page:

```razor
@page "/liveview-dashboard"
@using BlazorLiveView.Dashboard.Components
<LiveViewDashboard />
```

## Custom Dashboard

You can create a custom dashboard by only installing the [`BlazorLiveView.Core` package](https://www.nuget.org/packages/BlazorLiveView.Core) and getting the necessary information by injecting these services:
- `ICircuitTracker` - tracks and lists all active circuits (connections)
- `ILiveViewMirrorUriBuilder` - builds URI for the mirror endpoint
- `ICurrentCircuit` - provides id of the current circuit

## TODO
1. make mirror circuits really read-only (maybe custom hub + modified html skeleton)
2. patchers and reflection wrappers tests
3. patchers exceptions handling + logging
4. JS interop
5. identification of users in dashboard
6. dashboard UI upgrade: styling, filtering, ...
7. documentation/wiki

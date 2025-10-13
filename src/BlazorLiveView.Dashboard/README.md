# BlazorLiveView.Dashboard

A small dashboard UI for the BlazorLiveView library. See the main package: `BlazorLiveView`.

## Reference setup

Register necessary services in `Program.cs`:

```csharp
using BlazorLiveView.Dashboard.Extensions;
WebApplicationBuilder builder;
builder.AddLiveViewDashboardServices();
```

Include the `LiveViewDashboard` component to a Razor page:

```razor
@page "/liveview-dashboard"
<LiveViewDashboard />
```

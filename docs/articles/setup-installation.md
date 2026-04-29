# Setup: Installation

This is the first step of BlazorLiveView setup: **Installing the NuGet package**.

## Project Setup

BlazorLiveView requires:

- ASP.NET Core Blazor Server
- .NET 9.0 or later
- Global interactivity mode

It does not support:

- The older `AddServerSideBlazor`

### Creating a New Project

If starting a new project in Visual Studio, select a new `Blazor Web App` with the following settings:

- `Interactive render mode` to `Server` (WebAssembly components are not supported)
- `Interactivity location` to `Global`

## Package Installation

Install the `BlazorLiveView` NuGet package via the NuGet UI or the .NET CLI:

```bash
dotnet add package BlazorLiveView
```

This is a meta-package that includes both:

- BlazorLiveView.Core - Core services and mirror endpoint.
- BlazorLiveView.Dashboard - Pre-built dashboard components.

### Custom Dashboard

For custom dashboard implementations without the pre-built dashboard, you can install only the `BlazorLiveView.Core` package instead. See [Custom Dashboard](custom-dashboard.md) for more information.

## Next Steps

Continue with [Setup: Registering Services](setup-registering-services.md).

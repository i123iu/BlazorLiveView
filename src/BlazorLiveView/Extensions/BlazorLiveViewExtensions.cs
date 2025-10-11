using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Dashboard.Extensions;
using Microsoft.AspNetCore.Builder;

namespace BlazorLiveView.Extensions;

public static class BlazorLiveViewExtensions
{
    public static WebApplicationBuilder AddLiveViewBackendAndDashboard(
        this WebApplicationBuilder builder,
        Action<LiveViewOptions>? configureOptions = null
    )
    {
        return builder
            .AddLiveViewBackend(configureOptions)
            .AddLiveViewDashboardServices();
    }
}

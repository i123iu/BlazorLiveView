using BlazorLiveView.Dashboard.Circuits;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLiveView.Dashboard.Extensions;

public static class LiveViewDashboardExtensions
{
    public static WebApplicationBuilder AddLiveViewDashboardServices(
        this WebApplicationBuilder builder
    )
    {
        builder.Services
            .AddScoped<CircuitHandler, CurrentCircuitHandler>()
            .AddScoped<ICurrentCircuit, CurrentCircuit>();
        return builder;
    }
}

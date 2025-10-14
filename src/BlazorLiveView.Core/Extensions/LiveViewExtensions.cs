using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Patching;
using BlazorLiveView.Core.RenderTree;
using BlazorLiveView.Core.UriHelpers;
using HarmonyLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlazorLiveView.Core.Extensions;

public static class LiveViewExtensions
{
    /// <summary>
    /// Registers services required for <c>BlazorLiveView</c>. 
    /// </summary>
    public static WebApplicationBuilder AddLiveView(
        this WebApplicationBuilder builder,
        Action<LiveViewOptions>? configureOptions = null
    )
    {
        configureOptions ??= options => { };
        builder.Services.Configure(configureOptions);

        builder.Services
            .AddSingleton<ICircuitTracker, CircuitTracker>()
            .AddSingleton<CircuitHandler, LiveViewCircuitHandler>()
            .AddSingleton<IRenderTreeMirrorTranslatorFactory, RenderTreeMirrorTranslatorFactory>()
            .AddSingleton<ILiveViewMirrorUriBuilder, LiveViewMirrorUriBuilder>()
            .AddScoped<CircuitHandler, CurrentCircuitHandler>()
            .AddScoped<ICurrentCircuit, CurrentCircuit>()
            .Scan(scan => scan
                .FromAssemblyOf<IPatcher>()
                .AddClasses(classes => classes.AssignableTo<IPatcher>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

        return builder;
    }

    /// <summary>
    /// Finishes the setup of <c>BlazorLiveView</c> and maps the mirror 
    /// endpoint to the path specified in <see cref="LiveViewOptions.MirrorUri"/>. 
    /// </summary>
    public static WebApplication MapLiveViewMirrorEndpoint(
        this WebApplication app,
        Action<IEndpointConventionBuilder>? configureEndpoint = null
    )
    {
        Harmony harmony = new("BlazorLiveView");
        var patchers = app.Services.GetServices<IPatcher>();
        foreach (var patcher in patchers)
        {
            patcher.Patch(harmony);
        }

        var options = app.Services.GetRequiredService<IOptions<LiveViewOptions>>().Value;

        var mirrorEndpoint = app.MapGet(
            $"{options.MirrorUri.TrimEnd('/')}",
            MirrorEndpointController
        );
        configureEndpoint?.Invoke(mirrorEndpoint);

        return app;
    }

    private static async Task MirrorEndpointController(
        HttpContext context,
        [FromQuery(Name = "circuitId")] string circuitId
    )
    {
        var circuitTracker = context.RequestServices.GetRequiredService<ICircuitTracker>();

        var circuit = circuitTracker.GetCircuit(circuitId);
        if (circuit is null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Circuit with ID '{circuitId}' not found. ");
            return;
        }

        if (circuit is not IUserCircuit userCircuit)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"Cannot mirror a mirror circuit. ");
            return;
        }

        var circuitUri = userCircuit.Uri;
        using HttpClient httpClient = new();
        var response = await httpClient.GetAsync(circuitUri);
        var content = await response.Content.ReadAsStringAsync();
        await context.Response.WriteAsync(content);
    }
}

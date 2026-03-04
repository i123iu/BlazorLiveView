using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Patching;
using BlazorLiveView.Core.RenderTree;
using BlazorLiveView.Core.UriHelpers;
using HarmonyLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
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
            .AddSingleton<IRenderTreeTranslatorFactory, RenderTreeTranslatorFactory>()
            .AddSingleton<ILiveViewMirrorUriBuilder, LiveViewMirrorUriBuilder>()
            .AddSingleton<IPatchExceptionHandler, PatchExceptionHandler>()
            .AddScoped<CircuitHandler, CurrentCircuitHandler>()
            .AddScoped<ICurrentCircuit, CurrentCircuit>()
            .Scan(scan => scan
                .FromAssemblyOf<IPatcher>()
                .AddClasses(classes => classes.AssignableTo<IPatcher>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

        builder.Services.AddHttpForwarder();

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
            options.MirrorUri,
            MirrorEndpoint.HandleAsync
        );
        configureEndpoint?.Invoke(mirrorEndpoint);

        return app;
    }
}

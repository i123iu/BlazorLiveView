using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Patching;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using BlazorLiveView.Core.RenderTree;
using BlazorLiveView.Core.UriHelpers;
using HarmonyLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace BlazorLiveView.Core.Extensions;

public static class LiveViewExtensions
{
    /// <summary>
    /// Registers services required for <c>BlazorLiveView</c>. 
    /// </summary>
    public static ILiveViewBuilder AddLiveView(
        this IServerSideBlazorBuilder builder,
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
            .AddScoped<ILiveViewJSRuntime>(sp =>
            {
                var ijsRuntime = sp.GetRequiredService<IJSRuntime>();

                if (ijsRuntime is ILiveViewJSRuntime liveViewJSRuntime)
                {
                    // already wrapped
                    return liveViewJSRuntime;
                }

                if (ijsRuntime is not JSRuntime jsRuntime ||
                    !Types.RemoteJSRuntime.IsInstanceOfType(ijsRuntime))
                {
                    throw new InvalidOperationException(
                        $"Expected IJSRuntime to be of type RemoteJSRuntime, " +
                        $"but got {ijsRuntime.GetType().FullName}. "
                    );
                }

                return new LiveViewJSRuntime(
                    new(jsRuntime),
                    sp
                );
            })
            .Scan(scan => scan
                .FromAssemblyOf<IPatcher>()
                .AddClasses(classes => classes.AssignableTo<IPatcher>(), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

        builder.Services.AddHttpForwarder();

        return new LiveViewBuilder(builder.Services);
    }

    /// <summary>
    /// BlazorLiveView will intercept all JS interop invocations passed to
    /// <see cref="IJSRuntime"/> and will forward them to mirror circuits that
    /// are viewing the current user. To forward only specific invocations
    /// see <see cref="ILiveViewJSRuntime"/> (which does not require calling
    /// this extension method).
    /// </summary>
    public static ILiveViewBuilder InterceptJSInteropInvocations(
        this ILiveViewBuilder builder,
        Action<LiveViewJSInteropOptions>? configureOptions = null
    )
    {
        configureOptions ??= options => { };
        builder.Services.Configure(configureOptions);

        builder.Services
            .Decorate<IJSRuntime>((origRemoteJSRuntime, sp) =>
            {
                // Replace the default IJSRuntime implementation (RemoteJSRuntime)
                // with a new intercepting implementation (LiveViewJSRuntime)

                var jsInteropOptions = sp.GetRequiredService<IOptions<LiveViewJSInteropOptions>>();
                if (!jsInteropOptions.Value.InterceptIJSRuntime)
                    return origRemoteJSRuntime;

                RemoteJSRuntimeWrapper jsRuntime = new((JSRuntime)origRemoteJSRuntime);
                return new LiveViewJSRuntime(jsRuntime, sp);
            });

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

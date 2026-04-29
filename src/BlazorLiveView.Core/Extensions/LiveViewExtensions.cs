using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.JSInterop;
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
    /// Registers services required for BlazorLiveView.
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
                RemoteJSRuntimeWrapper remoteJSRuntime;
                if (ijsRuntime is RemoteJSRuntime intercepted)
                {
                    // The IJSRuntime has been wrapped (intercepted) by
                    // InterceptIJSRuntime.
                    remoteJSRuntime = intercepted.Inner;
                }
                else if (ijsRuntime is JSRuntime jsRuntime &&
                    Types.RemoteJSRuntime.IsInstanceOfType(ijsRuntime))
                {
                    // The IJSRuntime is the default RemoteJSRuntime.
                    remoteJSRuntime = new RemoteJSRuntimeWrapper(jsRuntime);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expected IJSRuntime to be of type RemoteJSRuntime, " +
                        $"but got {ijsRuntime.GetType().FullName}. "
                    );
                }

                return new LiveViewJSRuntime(
                    remoteJSRuntime,
                    sp.GetRequiredService<ICircuitTracker>(),
                    sp.GetRequiredService<ILogger<LiveViewJSRuntime>>()
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
    /// BlazorLiveView will intercept all <see cref="IJSRuntime"/> instances. 
    /// JS interop invocations passed to them will be forwarded to mirror
    /// circuits that are viewing the current user. To filter which invocations
    /// are forwarded, see <see cref="LiveViewJSInteropOptions.DefaultInterceptionBehaviour"/>
    /// and <see cref="LiveViewJSInteropOptions.InterceptionRules"/>.
    /// To forward only specific invocations use <see cref="ILiveViewJSRuntime"/>
    /// (which does not require calling this extension method).
    /// </summary>
    public static ILiveViewBuilder InterceptIJSRuntime(
        this ILiveViewBuilder builder,
        Action<LiveViewJSInteropOptions>? configureOptions = null
    )
    {
        configureOptions ??= options => { };
        builder.Services.Configure(configureOptions);
        builder.Services.Configure<LiveViewJSInteropOptions>(options =>
        {
            options.InterceptIJSRuntime = true;
        });

        builder.Services
            .Decorate<IJSRuntime>((origRemoteJSRuntime, sp) =>
            {
                // Decorate the default IJSRuntime implementation (RemoteJSRuntime)
                // with a new (intercepting) implementation (InterceptingRemoteJSRuntime).
                RemoteJSRuntimeWrapper remoteJSRuntime = new((JSRuntime)origRemoteJSRuntime);
                return new RemoteJSRuntime(
                    remoteJSRuntime,
                    sp.GetRequiredService<ICircuitTracker>(),
                    sp.GetRequiredService<ILogger<RemoteJSRuntime>>(),
                    sp.GetRequiredService<IOptions<LiveViewJSInteropOptions>>()
                );
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

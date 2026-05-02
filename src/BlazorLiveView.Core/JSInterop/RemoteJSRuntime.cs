using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorLiveView.Core.JSInterop;

/// <summary>
/// THIS IS NOT THE ORIGINAL RemoteJSRuntime. When MudBlazor checks if the current render state is prerender,
/// it checks jsRuntime.GetType().Name == "RemoteJSRuntime" (in src/MudBlazor/Extensions/IJSRuntimeExtensions.cs).
/// Rename only if that is resolved.
/// 
/// Wrapper around RemoteJSRuntime that is the default IJSRuntime implementation in Blazor Server.
/// Forwards InvokeAsync calls to mirror circuits, while checking the forwarding rules.
/// </summary>
internal class RemoteJSRuntime(
    RemoteJSRuntimeWrapper remoteJSRuntime,
    ICircuitTracker circuitTracker,
    ILogger<RemoteJSRuntime> logger,
    IOptions<LiveViewJSInteropOptions> liveViewJSInteropOptions
) : ForwardingJSRuntimeBase(
    remoteJSRuntime,
    circuitTracker,
    logger
)
{
    private readonly IOptions<LiveViewJSInteropOptions> _liveViewJSInteropOptions
        = liveViewJSInteropOptions;

    internal RemoteJSRuntimeWrapper Inner => _remoteJsRuntime;

    protected override bool ShouldForward(string identifier)
    {
        var jsInteropOptions = _liveViewJSInteropOptions.Value;
        bool shouldForward = jsInteropOptions.JsToDotnetForwardingRules.GetForwardingBehavior(identifier) switch
        {
            ForwardingBehavior.Forward => true,
            ForwardingBehavior.SkipForwarding => false,
            _ => throw new Exception()
        };

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Determined that JS invocation with identifier {Identifier} should {ForwardingBehavior}",
                identifier, shouldForward ? "be forwarded" : "NOT be forwarded"
            );
        }

        return shouldForward;
    }

    public static object ToRemoteJSRuntime(
        RemoteJSRuntime intercepted
    )
    {
        return intercepted._remoteJsRuntime.Inner;
    }
}

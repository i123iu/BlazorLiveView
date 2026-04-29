using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.JSInterop;

internal class LiveViewJSRuntime(
    RemoteJSRuntimeWrapper remoteJSRuntime,
    ICircuitTracker circuitTracker,
    ILogger<LiveViewJSRuntime> logger
) : ForwardingJSRuntimeBase(
    remoteJSRuntime,
    circuitTracker,
    logger
), ILiveViewJSRuntime
{
    public ValueTask<TValue> InvokeAsyncNoMirror<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, object?[]? args
    )
    {
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask<TValue> InvokeAsyncNoMirror<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, CancellationToken cancellationToken, object?[]? args
    )
    {
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, args);
    }

    protected override bool ShouldForward(string identifier)
    {
        // The user will use InvokeAsyncNoMirror if they want to skip mirroring.
        return true;
    }
}

using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.JSInterop;

internal abstract class ForwardingJSRuntimeBase(
    RemoteJSRuntimeWrapper remoteJsRuntime,
    ICircuitTracker circuitTracker,
    ILogger logger
) : IJSRuntime
{
    protected readonly ILogger _logger = logger;
    protected readonly RemoteJSRuntimeWrapper _remoteJsRuntime = remoteJsRuntime;
    protected readonly ICircuitTracker _circuitTracker = circuitTracker;

    [DebuggerHidden]
    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, object?[]? args
    )
    {
        ForwardInvocationToMirrors(identifier, CancellationToken.None, args);
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, args);
    }

    [DebuggerHidden]
    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, CancellationToken cancellationToken, object?[]? args
    )
    {
        ForwardInvocationToMirrors(identifier, cancellationToken, args);
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    private void ForwardInvocationToMirrors(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (!_remoteJsRuntime.IsInitialized)
        {
            // Calling InvokeAsync on RemoteJSRuntime will throw an exception
            // anyway, so mirroring it is not necessary.
            return;
        }

        if (!ShouldForward(identifier))
        {
            return;
        }

        ICircuit? circuit = _circuitTracker.GetCircuit(_remoteJsRuntime);
        if (circuit is null)
        {
            _logger.LogWarning(
                "No circuit found for the current JSRuntime. " +
                "Skipping mirror circuit invocation. "
            );
            return;
        }

        if (circuit is IUserCircuit userCircuit)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Notifying about JS interop method '{Identifier}' on circuit '{CircuitId}' with args: {Args}",
                    identifier, circuit.Id, args
                );
            }

            userCircuit.NotifyJSRuntimeInvoked(identifier, cancellationToken, args);
        }
    }

    protected abstract bool ShouldForward(string identifier);
}

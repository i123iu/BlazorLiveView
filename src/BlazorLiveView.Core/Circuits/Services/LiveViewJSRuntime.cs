using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.Circuits.Services;

internal class LiveViewJSRuntime : ILiveViewJSRuntime
{
    private readonly ILogger<LiveViewJSRuntime> _logger;
    private readonly ICircuitTracker _circuitTracker;
    private readonly RemoteJSRuntimeWrapper _remoteJsRuntime;

    public LiveViewJSRuntime(
        ILogger<LiveViewJSRuntime> logger,
        ICircuitTracker circuitTracker,
        IJSRuntime jsRuntime
    )
    {
        if (!Types.RemoteJSRuntime.IsInstanceOfType(jsRuntime))
        {
            throw new InvalidOperationException(
                $"Using {nameof(LiveViewJSRuntime)} requires an instance of " +
                $"{Types.RemoteJSRuntime.FullName} (which is provided by Blazor Server)."
            );
        }

        _logger = logger;
        _circuitTracker = circuitTracker;
        _remoteJsRuntime = new RemoteJSRuntimeWrapper((JSRuntime)jsRuntime);
    }

    private void NotifyInvocation(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        ICircuit? circuit = _circuitTracker.GetCircuit(_remoteJsRuntime);
        if (circuit is null)
        {
            _logger.LogWarning(
                "No circuit found for the current JSRuntime. " +
                "Skipping mirror circuit invocation. "
            );
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Invoking JS interop method '{Identifier}' on circuit '{CircuitId}' with args: {Args}",
                    identifier, circuit.Id, args
                );
            }

            if (circuit is IUserCircuit userCircuit)
            {
                userCircuit.NotifyJSRuntimeInvoked(identifier, cancellationToken, args);
            }
        }
    }

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, object?[]? args
    )
    {
        NotifyInvocation(identifier, CancellationToken.None, args);
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, CancellationToken cancellationToken, object?[]? args
    )
    {
        NotifyInvocation(identifier, cancellationToken, args);
        return _remoteJsRuntime.Inner.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }
}

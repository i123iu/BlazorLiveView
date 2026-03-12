using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.Circuits.Services;

internal class LiveViewJSRuntime(
    RemoteJSRuntimeWrapper remoteJSRuntime,
    IServiceProvider sp
) : ILiveViewJSRuntime
{
    private readonly ILogger<LiveViewJSRuntime> _logger = sp.GetRequiredService<ILogger<LiveViewJSRuntime>>();
    private readonly ICircuitTracker _circuitTracker = sp.GetRequiredService<ICircuitTracker>();
    private readonly RemoteJSRuntimeWrapper _remoteJsRuntime = remoteJSRuntime;

    private void NotifyInvocation(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        ICircuit? circuit = _circuitTracker.GetCircuit(_remoteJsRuntime);
        if (circuit is null)
        {
            _logger.LogWarning(
                "No circuit found for the current JSRuntime. " +
                "Skipping mirror circuit invocation. "
            );
            return;
        }

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

    public static object ToRemoteJSRuntime(
        LiveViewJSRuntime liveViewJSRuntime
    )
    {
        return liveViewJSRuntime._remoteJsRuntime.Inner;
    }
}

using BlazorLiveView.Core.JSInterop.DotnetToJs;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace BlazorLiveView.Core.Circuits.Services;

internal sealed class CircuitTracker(
    ILogger<CircuitTracker> logger,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory
) : ICircuitTracker
{
    public event ICircuitTracker.CircuitOpenedHandler? OnCircuitOpened;
    public event ICircuitTracker.CircuitClosedHandler? OnCircuitClosed;

    private readonly ILogger _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly ConcurrentDictionary<string, ICircuit> _circuitsById = new();
    private readonly ConcurrentDictionary<Renderer, ICircuit> _circuitsByRenderer = new();
    private readonly ConcurrentDictionary<JSRuntime, ICircuit> _circuitsByJsRuntime = new();

    /// <summary>
    /// Clients that have sent a HTTP request to the mirror endpoint, but are not yet opened. 
    /// </summary>
    private readonly ConcurrentDictionary<string, PendingMirrorCircuit>
        _pendingMirrorCircuitsById = new();

    private readonly struct PendingMirrorCircuit(
        IUserCircuit source,
        Guid? state,
        bool debugView
    )
    {
        public readonly IUserCircuit source = source;
        public readonly Guid? state = state;
        public readonly bool debugView = debugView;
    }

    public ICircuit? GetCircuit(string circuitId)
    {
        _circuitsById.TryGetValue(circuitId, out var circuit);
        return circuit;
    }

    public ICircuit? GetCircuit(RemoteRendererWrapper remoteRenderer)
    {
        _circuitsByRenderer.TryGetValue(remoteRenderer.Inner, out var circuit);
        return circuit;
    }

    public ICircuit? GetCircuit(RemoteJSRuntimeWrapper remoteJsRuntime)
    {
        _circuitsByJsRuntime.TryGetValue(remoteJsRuntime.Inner, out var circuit);
        return circuit;
    }

    public IReadOnlyCollection<ICircuit> ListCircuits()
    {
        return _circuitsById.Values.ToArray();
    }

    public void MirrorCircuitCreated(
        Circuit mirrorCircuit,
        IUserCircuit sourceCircuit,
        Guid? state,
        bool debugView
    )
    {
        if (_circuitsById.ContainsKey(mirrorCircuit.Id) ||
            _pendingMirrorCircuitsById.ContainsKey(mirrorCircuit.Id))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{mirrorCircuit.Id}' is already tracked."
            );
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Mirror circuit created: {CircuitId}, source: {SourceCircuitId}, state: {State}",
                mirrorCircuit.Id, sourceCircuit.Id, state
            );
        }

        PendingMirrorCircuit pending = new(sourceCircuit, state, debugView);
        if (!_pendingMirrorCircuitsById.TryAdd(mirrorCircuit.Id, pending))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{mirrorCircuit.Id}' is already tracked."
            );
        }
    }

    public void CircuitOpened(Circuit blazorCircuit)
    {
        if (_circuitsById.ContainsKey(blazorCircuit.Id))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is already tracked."
            );
        }

        ICircuit circuit;
        if (_pendingMirrorCircuitsById.TryRemove(blazorCircuit.Id, out var pending))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Mirror circuit opened: {CircuitId}",
                blazorCircuit.Id);
            }

            circuit = new MirrorCircuit(
                blazorCircuit,
                pending.source,
                pending.state,
                DateTime.UtcNow,
                pending.debugView,
                _serviceProvider.GetRequiredService<IPausedCircuitsTracker>(),
                _serviceProvider.GetRequiredService<IDotnetToJsArgsTranslator>(),
                _loggerFactory.CreateLogger<MirrorCircuit>(),
                _serviceProvider.GetRequiredService<IOptions<LiveViewJSInteropOptions>>()
            );
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("User circuit opened: {CircuitId}",
                blazorCircuit.Id);
            }

            circuit = new UserCircuit(
                blazorCircuit,
                DateTime.UtcNow,
                _serviceProvider.GetRequiredService<IPausedCircuitsTracker>(),
                _serviceProvider.GetRequiredService<IOptions<LiveViewOptions>>(),
                _loggerFactory.CreateLogger<UserCircuit>()
            );
        }

        if (!_circuitsById.TryAdd(blazorCircuit.Id, circuit))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is already tracked."
            );
        }

        CircuitWrapper circuitWrapped = new(blazorCircuit);

        Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
        if (!_circuitsByRenderer.TryAdd(renderer, circuit))
        {
            throw new InvalidOperationException(
                $"Circuit renderer is already tracked."
            );
        }

        JSRuntime jsRuntime = circuitWrapped.CircuitHost.JSRuntime.Inner;
        if (!_circuitsByJsRuntime.TryAdd(jsRuntime, circuit))
        {
            throw new InvalidOperationException(
                $"Circuit JSRuntime is already tracked."
            );
        }

        OnCircuitOpened?.Invoke(circuit);
    }

    public void CircuitUp(Circuit blazorCircuit)
    {
        if (!_circuitsById.TryGetValue(blazorCircuit.Id, out var circuit))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is not tracked or open."
            );
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Circuit up: {CircuitId}", blazorCircuit.Id);
        }
        circuit.SetUp();
    }

    public void CircuitDown(Circuit blazorCircuit)
    {
        if (!_circuitsById.TryGetValue(blazorCircuit.Id, out var circuit))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is not tracked."
            );
        }

        if (circuit.CircuitStatus == CircuitStatus.Down)
        {
            // Blazor sometimes calls CircuitDown twice when closing
            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Circuit down: {CircuitId}", blazorCircuit.Id);
        }
        circuit.SetDown();
    }

    public void CircuitClosed(Circuit blazorCircuit)
    {
        if (!_circuitsById.TryRemove(blazorCircuit.Id, out var circuit))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is not tracked or open."
            );
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Circuit closed: {CircuitId}", blazorCircuit.Id);
        }
        circuit.SetClosed();

        CircuitWrapper circuitWrapped = new(blazorCircuit);

        Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
        if (!_circuitsByRenderer.TryRemove(renderer, out _))
        {
            _logger.LogWarning(
                "Circuit renderer was not tracked when closing circuit '{CircuitId}'.",
                blazorCircuit.Id
            );
        }

        JSRuntime jsRuntime = circuitWrapped.CircuitHost.JSRuntime.Inner;
        if (!_circuitsByJsRuntime.TryRemove(jsRuntime, out _))
        {
            _logger.LogWarning(
                "Circuit JSRuntime was not tracked when closing circuit '{CircuitId}'.",
                blazorCircuit.Id
            );
        }

        OnCircuitClosed?.Invoke(circuit);
        circuit.Dispose();
    }
}

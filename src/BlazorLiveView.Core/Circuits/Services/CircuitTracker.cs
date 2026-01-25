using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BlazorLiveView.Core.Circuits.Services;

internal sealed class CircuitTracker(
    ILogger<CircuitTracker> logger,
    ILoggerFactory loggerFactory
) : ICircuitTracker
{
    public event ICircuitTracker.CircuitOpenedHandler? CircuitOpenedEvent;
    public event ICircuitTracker.CircuitClosedHandler? CircuitClosedEvent;

    private readonly ILogger<CircuitTracker> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly ConcurrentDictionary<string, ICircuit> _circuitsById = new();
    private readonly ConcurrentDictionary<Renderer, ICircuit> _circuitsByRenderer = new();

    private readonly struct PendingMirrorCircuit(
        IUserCircuit source,
        IUserCircuit? parent,
        bool debugView
    )
    {
        public readonly IUserCircuit source = source;
        public readonly IUserCircuit? parent = parent;
        public readonly bool debugView = debugView;
    }

    /// <summary>
    /// Clients that have sent a HTTP request to the mirror endpoint, but are not yet opened. 
    /// </summary>
    private readonly ConcurrentDictionary<string, PendingMirrorCircuit> _pendingMirrorCircuitsById = new();

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

    public IReadOnlyCollection<ICircuit> ListCircuits()
    {
        return _circuitsById.Values.ToArray();
    }

    public void MirrorCircuitCreated(
        Circuit mirrorCircuit,
        IUserCircuit sourceCircuit, 
        IUserCircuit? parentCircuit, 
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

        _logger.LogDebug(
            "Mirror circuit created: {CircuitId}, source: {SourceCircuitId}, parent: {ParentCircuitId}",
            mirrorCircuit.Id, sourceCircuit.Id, parentCircuit?.Id
        );

        PendingMirrorCircuit pending = new(sourceCircuit, parentCircuit, debugView);
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
            _logger.LogDebug("Mirror circuit opened: {CircuitId}",
                blazorCircuit.Id);

            circuit = new MirrorCircuit(
                blazorCircuit,
                pending.source,
                pending.parent,
                DateTime.UtcNow,
                pending.debugView,
                _loggerFactory.CreateLogger<MirrorCircuit>()
            );
        }
        else
        {
            _logger.LogDebug("User circuit opened: {CircuitId}",
                blazorCircuit.Id);

            circuit = new UserCircuit(
                blazorCircuit,
                DateTime.UtcNow,
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

        CircuitOpenedEvent?.Invoke(circuit);
    }

    public void CircuitUp(Circuit blazorCircuit)
    {
        if (!_circuitsById.TryGetValue(blazorCircuit.Id, out var circuit))
        {
            throw new InvalidOperationException(
                $"Circuit with id '{blazorCircuit.Id}' is not tracked or open."
            );
        }

        _logger.LogDebug("Circuit up: {CircuitId}", blazorCircuit.Id);
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
            // Blazor can call CircuitDown twice when closing
            return;
        }

        _logger.LogDebug("Circuit down: {CircuitId}", blazorCircuit.Id);
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

        _logger.LogDebug("Circuit closed: {CircuitId}", blazorCircuit.Id);
        circuit.SetClosed();

        CircuitWrapper circuitWrapped = new(blazorCircuit);
        Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
        _circuitsByRenderer.TryRemove(renderer, out _);

        CircuitClosedEvent?.Invoke(circuit);
    }
}

using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Circuits;

internal sealed class CircuitTracker(
    ILogger<CircuitTracker> logger
) : ICircuitTracker
{
    public event ICircuitTracker.CircuitOpenedHandler? CircuitOpenedEvent;
    public event ICircuitTracker.CircuitClosedHandler? CircuitClosedEvent;

    private readonly Lock _lock = new();
    private readonly ILogger<CircuitTracker> _logger = logger;

    private readonly Dictionary<string, TrackedCircuit> _circuitsById = new();
    private readonly Dictionary<Renderer, TrackedCircuit> _circuitsByRenderer = new();

    /// <summary>
    /// Circuits that target the mirror endpoint, but are not yet opened. 
    /// Maps mirror's circuit id to the source circuit. 
    /// </summary>
    private readonly Dictionary<string, IRegularCircuit> _pendingMirrorCircuits = new();

    public TrackedCircuit? GetCircuit(string circuitId)
    {
        lock (_lock)
        {
            return _circuitsById.GetValueOrDefault(circuitId);
        }
    }

    TrackedCircuit? ICircuitTracker.GetCircuit(RemoteRendererWrapper remoteRenderer)
    {
        lock (_lock)
        {
            return _circuitsByRenderer.GetValueOrDefault(remoteRenderer.Inner);
        }
    }

    public IReadOnlyCollection<TrackedCircuit> ListCircuits()
    {
        lock (_lock)
        {
            return _circuitsById.Values.ToArray();
        }
    }

    public IReadOnlyCollection<IMirrorCircuit> ListMirrorCircuits()
    {
        lock (_lock)
        {
            return _circuitsById.Values
                .Where(c => c.IsMirror)
                .Select(c => c.MirrorCircuit!)
                .ToArray();
        }
    }

    public IReadOnlyCollection<IRegularCircuit> ListRegularCircuits()
    {
        lock (_lock)
        {
            return _circuitsById.Values
                .Where(c => !c.IsMirror)
                .Select(c => c.RegularCircuit!)
                .ToArray();
        }
    }

    public void MirrorCircuitCreated(Circuit mirrorCircuit, IRegularCircuit sourceCircuit)
    {
        lock (_lock)
        {
            _logger.LogInformation(
                "Mirror circuit created: {CircuitId}, source: {SourceCircuitId}",
                mirrorCircuit.Id, sourceCircuit.Id
            );
            _pendingMirrorCircuits.Add(mirrorCircuit.Id, sourceCircuit);
        }
    }

    public void CircuitOpened(Circuit circuit)
    {
        lock (_lock)
        {
            if (_circuitsById.ContainsKey(circuit.Id))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{circuit.Id}' is already registered as open."
                );
            }

            TrackedCircuit trackedCircuit;
            if (_pendingMirrorCircuits.Remove(circuit.Id, out var sourceCircuit))
            {
                _logger.LogInformation("Mirror circuit opened: {CircuitId}",
                    circuit.Id);

                MirrorCircuit mirrorCircuit = new(circuit, sourceCircuit);
                trackedCircuit = new TrackedCircuit(mirrorCircuit);
            }
            else
            {
                _logger.LogInformation("Regular circuit opened: {CircuitId}",
                    circuit.Id);

                RegularCircuit regularCircuit = new(circuit);
                trackedCircuit = new TrackedCircuit(regularCircuit);
            }

            _circuitsById.Add(circuit.Id, trackedCircuit);

            CircuitWrapper circuitWrapped = new(circuit);
            Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
            _circuitsByRenderer.Add(renderer, trackedCircuit);

            CircuitOpenedEvent?.Invoke(trackedCircuit);
        }
    }

    public void CircuitClosed(Circuit circuit)
    {
        lock (_lock)
        {
            if (!_circuitsById.Remove(circuit.Id, out var trackedCircuit))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{circuit.Id}' is not tracked as open."
                );
            }

            _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);

            CircuitWrapper circuitWrapped = new(circuit);
            Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
            _circuitsByRenderer.Remove(renderer);

            CircuitClosedEvent?.Invoke(trackedCircuit);
        }
    }
}

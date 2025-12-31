using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Circuits;

internal sealed class CircuitTracker(
    ILogger<CircuitTracker> logger,
    ILoggerFactory loggerFactory
) : ICircuitTracker
{
    public event ICircuitTracker.CircuitOpenedHandler? CircuitOpenedEvent;
    public event ICircuitTracker.CircuitClosedHandler? CircuitClosedEvent;

    private readonly Lock _lock = new();
    private readonly ILogger<CircuitTracker> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly Dictionary<string, ICircuit> _circuitsById = new();
    private readonly Dictionary<Renderer, ICircuit> _circuitsByRenderer = new();

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
    private readonly Dictionary<string, PendingMirrorCircuit> _pendingMirrorCircuitsById = new();

    public ICircuit? GetCircuit(string circuitId)
    {
        lock (_lock)
        {
            return _circuitsById.GetValueOrDefault(circuitId);
        }
    }

    public ICircuit? GetCircuit(RemoteRendererWrapper remoteRenderer)
    {
        lock (_lock)
        {
            return _circuitsByRenderer.GetValueOrDefault(remoteRenderer.Inner);
        }
    }

    public IReadOnlyCollection<ICircuit> ListCircuits()
    {
        lock (_lock)
        {
            return _circuitsById.Values.ToArray();
        }
    }

    public void MirrorCircuitCreated(Circuit mirrorCircuit,
        IUserCircuit sourceCircuit, IUserCircuit? parentCircuit, bool debugView)
    {
        lock (_lock)
        {
            if (_circuitsById.ContainsKey(mirrorCircuit.Id) ||
                _pendingMirrorCircuitsById.ContainsKey(mirrorCircuit.Id))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{mirrorCircuit.Id}' is already tracked."
                );
            }

            _logger.LogInformation(
                "Mirror circuit created: {CircuitId}, source: {SourceCircuitId}, parent: {ParentCircuitId}",
                mirrorCircuit.Id, sourceCircuit.Id, parentCircuit?.Id
            );
            PendingMirrorCircuit pending = new(sourceCircuit, parentCircuit, debugView);
            _pendingMirrorCircuitsById.Add(mirrorCircuit.Id, pending);
        }
    }

    public void CircuitOpened(Circuit blazorCircuit)
    {
        lock (_lock)
        {
            if (_circuitsById.ContainsKey(blazorCircuit.Id))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{blazorCircuit.Id}' is already tracked."
                );
            }

            ICircuit circuit;
            if (_pendingMirrorCircuitsById.Remove(blazorCircuit.Id, out var pending))
            {
                _logger.LogInformation("Mirror circuit opened: {CircuitId}",
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
                _logger.LogInformation("User circuit opened: {CircuitId}",
                    blazorCircuit.Id);

                circuit = new UserCircuit(
                    blazorCircuit,
                    DateTime.UtcNow,
                    _loggerFactory.CreateLogger<UserCircuit>()
                );
            }

            _circuitsById.Add(blazorCircuit.Id, circuit);

            CircuitWrapper circuitWrapped = new(blazorCircuit);
            Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
            _circuitsByRenderer.Add(renderer, circuit);

            CircuitOpenedEvent?.Invoke(circuit);
        }
    }

    public void CircuitUp(Circuit blazorCircuit)
    {
        lock (_lock)
        {
            if (!_circuitsById.TryGetValue(blazorCircuit.Id, out var circuit))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{blazorCircuit.Id}' is not tracked or open."
                );
            }

            _logger.LogInformation("Circuit up: {CircuitId}", blazorCircuit.Id);
            circuit.SetUp();
        }
    }

    public void CircuitDown(Circuit blazorCircuit)
    {
        lock (_lock)
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

            _logger.LogInformation("Circuit down: {CircuitId}", blazorCircuit.Id);
            circuit.SetDown();
        }
    }

    public void CircuitClosed(Circuit blazorCircuit)
    {
        lock (_lock)
        {
            if (!_circuitsById.Remove(blazorCircuit.Id, out var circuit))
            {
                throw new InvalidOperationException(
                    $"Circuit with id '{blazorCircuit.Id}' is not tracked or open."
                );
            }

            _logger.LogInformation("Circuit closed: {CircuitId}", blazorCircuit.Id);
            circuit.SetClosed();

            CircuitWrapper circuitWrapped = new(blazorCircuit);
            Renderer renderer = circuitWrapped.CircuitHost.Renderer.Inner;
            _circuitsByRenderer.Remove(renderer);

            CircuitClosedEvent?.Invoke(circuit);
        }
    }
}

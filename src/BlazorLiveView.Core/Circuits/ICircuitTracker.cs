using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// Tracks all active circuits (SignalR connections) with users (their browser tabs). 
/// </summary>
public interface ICircuitTracker
{
    public delegate void CircuitOpenedHandler(TrackedCircuit circuit);
    public delegate void CircuitClosedHandler(TrackedCircuit circuit);

    event CircuitOpenedHandler? CircuitOpenedEvent;
    event CircuitClosedHandler? CircuitClosedEvent;

    TrackedCircuit? GetCircuit(string circuitId);
    internal TrackedCircuit? GetCircuit(RemoteRendererWrapper remoteRenderer);

    IReadOnlyCollection<TrackedCircuit> ListCircuits();
    IReadOnlyCollection<IMirrorCircuit> ListMirrorCircuits();
    IReadOnlyCollection<IRegularCircuit> ListRegularCircuits();

    internal void MirrorCircuitCreated(Circuit mirrorCircuit, IRegularCircuit sourceCircuit);
    internal void CircuitOpened(Circuit circuit);
    internal void CircuitClosed(Circuit circuit);
}

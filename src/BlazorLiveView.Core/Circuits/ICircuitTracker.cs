using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// Tracks all active SignalR connections with clients (their browser tabs).
/// </summary>
public interface ICircuitTracker
{
    public delegate void CircuitOpenedHandler(ICircuit circuit);
    public delegate void CircuitClosedHandler(ICircuit circuit);

    event CircuitOpenedHandler? CircuitOpenedEvent;
    event CircuitClosedHandler? CircuitClosedEvent;

    ICircuit? GetCircuit(string id);
    internal ICircuit? GetCircuit(RemoteRendererWrapper remoteRenderer);

    IReadOnlyCollection<ICircuit> ListCircuits();

    /// <summary>
    /// Called when a mirror circuit's constructor was called.
    /// </summary>
    internal void MirrorCircuitCreated(Circuit mirrorCircuit, IUserCircuit sourceCircuit);

    /// <summary>
    /// Called when any circuit's SignalR connection was initialized.
    /// </summary>
    internal void CircuitOpened(Circuit circuit);

    /// <summary>
    /// Called after <see cref="CircuitOpened"/>, or after reconnecting. 
    /// </summary>
    internal void CircuitUp(Circuit circuit);

    /// <summary>
    /// Called when a circuit's SignalR connection was lost (could still be reconnected.
    /// </summary>
    internal void CircuitDown(Circuit circuit);

    /// <summary>
    /// Called when a circuit's SignalR connection was closed.
    /// </summary>
    internal void CircuitClosed(Circuit circuit);
}

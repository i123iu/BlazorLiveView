using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// Connection to a client, either a <see cref="IUserCircuit"/> or a 
/// <see cref="IMirrorCircuit"/>.
/// </summary>
public interface ICircuit
{
    public delegate void CircuitStatusChangedHandler(ICircuit circuit);

    event CircuitStatusChangedHandler? CircuitStatusChanged;

    string Id { get; }
    CircuitStatus CircuitStatus { get; }
    DateTime OpenedAt { get; }

    internal void SetUp();
    internal void SetDown();
    internal void SetClosed();

    internal ComponentState? GetOptionalComponentState(int componentId);
}

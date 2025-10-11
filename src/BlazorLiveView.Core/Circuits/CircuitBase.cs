using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal abstract class CircuitBase(
    Circuit circuit,
    DateTime openedAt
) : ICircuit
{
    public event ICircuit.CircuitStatusChangedHandler? CircuitStatusChanged;

    private CircuitStatus _circuitStatus = CircuitStatus.Open;

    protected CircuitWrapper Circuit { get; } = new(circuit);

    public string Id { get; } = circuit.Id;
    public DateTime OpenedAt { get; set; } = openedAt;
    public CircuitStatus CircuitStatus => _circuitStatus;

    public void SetUp()
    {
        if (_circuitStatus == CircuitStatus.Up)
            return;
        _circuitStatus = CircuitStatus.Up;
        CircuitStatusChanged?.Invoke(this);
    }

    public void SetDown()
    {
        if (_circuitStatus == CircuitStatus.Down)
            return;
        _circuitStatus = CircuitStatus.Down;
        CircuitStatusChanged?.Invoke(this);
    }

    public void SetClosed()
    {
        if (_circuitStatus == CircuitStatus.Closed)
            return;
        _circuitStatus = CircuitStatus.Closed;
        CircuitStatusChanged?.Invoke(this);
    }

    public ComponentState? GetOptionalComponentState(int componentId)
    {
        return Circuit.CircuitHost.Renderer.GetOptionalComponentState(componentId);
    }
}

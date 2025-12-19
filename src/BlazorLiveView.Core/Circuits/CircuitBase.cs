using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Circuits;

internal abstract class CircuitBase(
    Circuit circuit,
    DateTime openedAt,
    ILogger<CircuitBase> logger
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
        switch (_circuitStatus)
        {
            case CircuitStatus.Open:
            case CircuitStatus.Down:
                _circuitStatus = CircuitStatus.Up;
                CircuitStatusChanged?.Invoke(this);
                break;

            case CircuitStatus.Up:
            case CircuitStatus.Closed:
                InvalidStatusChange(CircuitStatus.Up);
                return;

            default: throw new NotImplementedException();
        }
    }

    public void SetDown()
    {
        switch (_circuitStatus)
        {
            case CircuitStatus.Open:
            case CircuitStatus.Up:
                _circuitStatus = CircuitStatus.Down;
                CircuitStatusChanged?.Invoke(this);
                break;

            case CircuitStatus.Down:
            case CircuitStatus.Closed:
                InvalidStatusChange(CircuitStatus.Down);
                return;

            default: throw new NotImplementedException();
        }
    }

    public void SetClosed()
    {
        switch (_circuitStatus)
        {
            case CircuitStatus.Open:
            case CircuitStatus.Up:
            case CircuitStatus.Down:
                _circuitStatus = CircuitStatus.Closed;
                CircuitStatusChanged?.Invoke(this);
                break;

            case CircuitStatus.Closed:
                InvalidStatusChange(CircuitStatus.Closed);
                return;

            default: throw new NotImplementedException();
        }
    }

    private void InvalidStatusChange(CircuitStatus newStatus)
    {
        // Only log a warning for now
        // TODO: make this an exception
        logger.LogWarning(
            "Tried to change circuit's state state changed from {Old} to {New} (id={Id}) ",
            _circuitStatus, newStatus, Id
        );
    }

    public ComponentState? GetOptionalComponentState(int componentId)
    {
        return Circuit.CircuitHost.Renderer.GetOptionalComponentState(componentId);
    }
}

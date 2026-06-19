using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

internal class CurrentCircuit(ICircuitTracker circuitTracker)
    : ICurrentCircuit
{
    private readonly ICircuitTracker _circuitTracker = circuitTracker;

    public Circuit? Current { get; set; }

    public ICircuit? AsICircuit()
    {
        if (Current is null) return null;
        return _circuitTracker.GetCircuit(Current.Id);
    }

    public IUserCircuit? AsUserCircuit() => Current is null ? null :
        (IUserCircuit?)_circuitTracker.GetCircuit(Current.Id);
}

using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

internal class CurrentCircuit(ICircuitTracker circuitTracker)
    : ICurrentCircuit
{
    private readonly ICircuitTracker _circuitTracker = circuitTracker;

    public Circuit? Current { get; set; }

    public ICircuit? AsLiveViewCircuit()
        => Current is null
            ? null
            : _circuitTracker.GetCircuit(Current.Id);

    public IUserCircuit? AsUserCircuit() => Current is null ? null :
        (IUserCircuit?)_circuitTracker.GetCircuit(Current.Id);
}

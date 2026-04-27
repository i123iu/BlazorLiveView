using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

internal class CurrentCircuit(ICircuitTracker circuitTracker)
    : ICurrentCircuit
{
    private readonly ICircuitTracker _circuitTracker = circuitTracker;

    public Circuit? Current { get; set; }

    public IUserCircuit? AsUserCircuit() => Current is null ? null :
        (IUserCircuit?)_circuitTracker.GetCircuit(Current.Id);
}

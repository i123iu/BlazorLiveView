using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Dashboard.Circuits;

internal class CurrentCircuit : ICurrentCircuit
{
    public Circuit? Current { get; set; }
}

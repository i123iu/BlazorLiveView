using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal class CurrentCircuit : ICurrentCircuit
{
    public Circuit? Current { get; set; }
}

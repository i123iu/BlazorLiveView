using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

internal class CurrentCircuit : ICurrentCircuit
{
    public Circuit? Current { get; set; }
}

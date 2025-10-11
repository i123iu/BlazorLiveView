using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Dashboard.Circuits;

public interface ICurrentCircuit
{
    Circuit? Current { get; set; }
}

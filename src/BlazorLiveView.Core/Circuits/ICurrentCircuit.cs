using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

public interface ICurrentCircuit
{
    Circuit? Current { get; set; }
}

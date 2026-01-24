using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

public interface ICurrentCircuit
{
    Circuit? Current { get; set; }
}

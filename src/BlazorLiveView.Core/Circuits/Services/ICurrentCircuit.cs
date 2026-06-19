using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits.Services;

/// <summary>
/// Can be used to access the circuit of the current scope.
/// </summary>
public interface ICurrentCircuit
{
    /// <summary>
    /// Returns the circuit of the current scope.
    /// </summary>
    Circuit? Current { get; set; }

    public ICircuit? AsICircuit();

    /// <summary>
    /// Returns the <see cref="IUserCircuit"/> for the current scope (circuit).
    /// </summary>
    public IUserCircuit? AsUserCircuit();
}

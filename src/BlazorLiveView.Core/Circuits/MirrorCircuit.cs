using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit(
    Circuit circuit,
    IUserCircuit source
) : IMirrorCircuit
{
    private readonly CircuitWrapper _circuit = new(circuit);
    private readonly IUserCircuit _source = source;

    public string Id => _circuit.Inner.Id;
    public IUserCircuit SourceCircuit => _source;

    ComponentState IMirrorCircuit.GetComponentState(int componentId)
    {
        return _circuit.CircuitHost.Renderer.GetComponentState(componentId);
    }
}
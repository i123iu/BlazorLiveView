using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit(
    Circuit circuit,
    IUserCircuit source,
    DateTime openedAt
) : CircuitBase(circuit, openedAt), IMirrorCircuit
{
    private readonly IUserCircuit _source = source;

    public IUserCircuit SourceCircuit => _source;

    public ComponentState GetComponentState(int componentId)
    {
        return Circuit.CircuitHost.Renderer.GetComponentState(componentId);
    }
}
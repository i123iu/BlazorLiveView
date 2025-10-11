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
}
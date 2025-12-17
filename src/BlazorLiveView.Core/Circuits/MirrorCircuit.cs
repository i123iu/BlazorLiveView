using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit(
    Circuit circuit,
    IUserCircuit source,
    DateTime openedAt,
    bool debugView
) : CircuitBase(circuit, openedAt), IMirrorCircuit
{
    private readonly IUserCircuit _source = source;
    private readonly bool _debugView = debugView;

    public IUserCircuit SourceCircuit => _source;
    public bool DebugView => _debugView;
}
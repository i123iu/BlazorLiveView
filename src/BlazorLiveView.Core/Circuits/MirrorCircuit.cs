using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit(
    Circuit circuit,
    IUserCircuit source,
    DateTime openedAt,
    bool debugView,
    ILogger<MirrorCircuit> logger
) : CircuitBase(circuit, openedAt, logger), IMirrorCircuit
{
    private readonly IUserCircuit _source = source;
    private readonly bool _debugView = debugView;
    private MirrorCircuitBlockReason? _blockReason;

    public event IMirrorCircuit.MirrorCircuitBlockedHandler? MirrorCircuitBlocked;

    public IUserCircuit SourceCircuit => _source;
    public bool DebugView => _debugView;

    public bool IsBlocked => _blockReason != null;
    public MirrorCircuitBlockReason BlockReason => _blockReason
        ?? throw new Exception("Not blocked");

    public void SetBlocked(MirrorCircuitBlockReason blockReason)
    {
        logger.LogInformation("Blocking mirror circuit id={Id} because '{ReasonMessage}'",
            Id, blockReason.message);
        _blockReason = blockReason;
        MirrorCircuitBlocked?.Invoke(this);
    }
}
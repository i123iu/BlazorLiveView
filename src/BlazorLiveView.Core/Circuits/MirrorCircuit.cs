using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit : CircuitBase, IMirrorCircuit
{
    private readonly ILogger _logger;
    private readonly IUserCircuit _source;
    private readonly Guid? _state;
    private readonly bool _debugView;
    private MirrorCircuitBlockReason? _blockReason = null;

    public event IMirrorCircuit.MirrorCircuitBlockedHandler? MirrorCircuitBlocked;

    public IUserCircuit SourceCircuit => _source;
    public Guid? State => _state;
    public bool DebugView => _debugView;

    public bool IsBlocked => _blockReason != null;
    public MirrorCircuitBlockReason BlockReason => _blockReason
        ?? throw new Exception("Not blocked");

    public MirrorCircuit(
        Circuit circuit,
        IUserCircuit source,
        Guid? state,
        DateTime openedAt,
        bool debugView,
        ILogger<MirrorCircuit> logger
    ) : base(circuit, openedAt, logger)
    {
        _logger = logger;
        _source = source;
        _state = state;
        _debugView = debugView;

        _source.JSRuntimeInvoked += Source_JSRuntimeInvoked;
    }

    public override void Dispose()
    {
        _source.JSRuntimeInvoked -= Source_JSRuntimeInvoked;
        base.Dispose();
    }

    private async void Source_JSRuntimeInvoked(
        IUserCircuit _,
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    )
    {
        if (CircuitStatus == CircuitStatus.Closed) return;
        if (IsBlocked) return;

        try
        {
            var jsRuntime = Circuit.CircuitHost.JSRuntime;
            await Task.Delay(100);
            await jsRuntime.Inner.InvokeVoidAsync(identifier, cancellationToken, args);
        }
        catch (Exception ex)
        {
            // TODO:  Either block the mirror circuit or at least notify the mirror circuit's user (admin)
            _logger.LogError(
                ex,
                "Failed to invoke JS runtime method '{Identifier}' on mirror circuit id={Id}",
                identifier, Id
            );
        }
    }

    public void SetBlocked(MirrorCircuitBlockReason blockReason)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Blocking mirror circuit id={Id} because '{ReasonMessage}'",
                Id, blockReason.message);
        }
        _blockReason = blockReason;
        MirrorCircuitBlocked?.Invoke(this);
    }
}
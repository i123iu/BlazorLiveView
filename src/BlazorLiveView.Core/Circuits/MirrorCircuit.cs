using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Threading.Channels;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit : CircuitBase, IMirrorCircuit
{
    private readonly ILogger _logger;
    private readonly IUserCircuit _source;
    private readonly Guid? _state;
    private readonly bool _debugView;
    private MirrorCircuitBlockReason? _blockReason = null;

    private readonly Channel<JSInvocation> _jsInvocationQueue;
    private readonly Task _processingTask;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly record struct JSInvocation(
        string Identifier,
        CancellationToken CancellationToken,
        object?[]? Args
    );

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
        _cancellationTokenSource = new CancellationTokenSource();
        _jsInvocationQueue = Channel.CreateUnbounded<JSInvocation>(new()
        {
            SingleReader = true,
            SingleWriter = false
        });

        _source.JSRuntimeInvoked += Source_JSRuntimeInvoked;
        CircuitStatusChanged += OnCircuitStatusChanged;
        _processingTask = ProcessInvocationsAsync(_cancellationTokenSource.Token);
    }

    public override void Dispose()
    {
        _source.JSRuntimeInvoked -= Source_JSRuntimeInvoked;
        CircuitStatusChanged -= OnCircuitStatusChanged;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        base.Dispose();
    }

    private void Source_JSRuntimeInvoked(
        IUserCircuit _,
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    )
    {
        if (_cancellationTokenSource.IsCancellationRequested) return;

        _jsInvocationQueue.Writer.TryWrite(new JSInvocation(identifier, cancellationToken, args));
    }

    private void OnCircuitStatusChanged(ICircuit circuit)
    {
        if (circuit.CircuitStatus == CircuitStatus.Closed)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    private async Task ProcessInvocationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var invocation in _jsInvocationQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken,
                        invocation.CancellationToken
                    );

                    var jsRuntime = Circuit.CircuitHost.JSRuntime;

                    // A short delay so that components can finish rendering.
                    // For example when IJSRuntime.InvokeAsync is called in
                    // OnAfterRenderAsync.
                    await Task.Delay(100, linkedCts.Token);

                    await jsRuntime.Inner.InvokeVoidAsync(
                        invocation.Identifier,
                        linkedCts.Token,
                        invocation.Args
                    );
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                }
                catch (Exception ex)
                {
                    // TODO:  Either block the mirror circuit or at least notify the mirror circuit's user (admin)
                    _logger.LogError(
                        ex,
                        "Failed to invoke JS runtime method '{Identifier}' on mirror circuit id={Id}",
                        invocation.Identifier, Id
                    );
                }
            }
        }
        catch (OperationCanceledException)
        { }
    }

    public void SetBlocked(MirrorCircuitBlockReason blockReason)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Blocking mirror circuit id={Id} because '{ReasonMessage}'",
                Id, blockReason.message);
        }
        _blockReason = blockReason;
        _cancellationTokenSource.Cancel();
        MirrorCircuitBlocked?.Invoke(this);
    }
}
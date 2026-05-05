using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Components.Tools;
using BlazorLiveView.Core.JSInterop;
using BlazorLiveView.Core.Options;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Threading.Channels;

namespace BlazorLiveView.Core.Circuits;

internal sealed class MirrorCircuit : CircuitBase, IMirrorCircuit
{
    public event IMirrorCircuit.MirrorCircuitBlockedHandler? MirrorCircuitBlocked;
    public event IMirrorCircuit.WindowSizeSyncChangedHandler? WindowSizeSyncChanged;
    public event IMirrorCircuit.ScrollSyncChangedHandler? ScrollSyncChanged;
    public event IMirrorCircuit.PointerSyncChangedHandler? PointerSyncChanged;
    public event IMirrorCircuit.CursorPositionChangedHandler? CursorPositionChanged;

    public IUserCircuit SourceCircuit { get; private set; }
    public Guid? State { get; private set; }
    public bool DebugView { get; private set; }
    public bool WindowSizeSyncEnabled { get; private set; } = false;
    public bool ScrollSyncEnabled { get; private set; } = false;
    public bool LaserPointerEnabled { get; private set; } = false;
    public Position? CursorPosition { get; private set; } = null;

    public bool IsBlocked => _blockReason != null;
    public MirrorCircuitBlockReason BlockReason => _blockReason
        ?? throw new Exception("Not blocked");

    private readonly ILogger _logger;
    private readonly IPausedCircuitsTracker _pausedCircuitsTracker;
    private readonly LiveViewJSInteropOptions _liveViewJSInteropOptions;
    private readonly IDotnetToJsArgsTranslator _dotnetToJsArgsTranslator;

    private readonly Channel<JSInvocation> _jsInvocationQueue;
    private readonly Task? _processingTask;
    private readonly record struct JSInvocation(
        string Identifier,
        CancellationToken CancellationToken,
        object?[]? Args,
        long CreatedAtTicks
    );

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private MirrorCircuitBlockReason? _blockReason = null;

    public MirrorCircuit(
        Circuit circuit,
        IUserCircuit source,
        Guid? state,
        DateTime openedAt,
        bool debugView,
        IPausedCircuitsTracker pausedCircuitsTracker,
        IDotnetToJsArgsTranslator dotnetToJsArgsTranslator,
        ILogger<MirrorCircuit> logger,
        IOptions<LiveViewJSInteropOptions> liveViewJSInteropOptions
    ) : base(circuit, openedAt, logger)
    {
        SourceCircuit = source;
        SourceCircuit.NotifyMirrorCircuitAdded(this);

        State = state;
        DebugView = debugView;
        _logger = logger;
        _dotnetToJsArgsTranslator = dotnetToJsArgsTranslator;
        _pausedCircuitsTracker = pausedCircuitsTracker;
        _liveViewJSInteropOptions = liveViewJSInteropOptions.Value;

        _jsInvocationQueue = Channel.CreateUnbounded<JSInvocation>(new()
        {
            SingleReader = true,
            SingleWriter = false
        });

        SourceCircuit.JSRuntimeInvoked += Source_JSRuntimeInvoked;
        CircuitStatusChanged += OnCircuitStatusChanged;

        if (!DebugView)
        {
            // Do not forward JS invocations to debug views, since all will fail anyway.
            _processingTask = ProcessInvocationsAsync(_cancellationTokenSource.Token);
        }
    }

    public override void Dispose()
    {
        SourceCircuit.NotifyMirrorCircuitRemoved(this);
        SourceCircuit.JSRuntimeInvoked -= Source_JSRuntimeInvoked;
        CircuitStatusChanged -= OnCircuitStatusChanged;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        base.Dispose();
    }

    private void Source_JSRuntimeInvoked(
        IUserCircuit userCircuit,
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    )
    {
        if (_cancellationTokenSource.IsCancellationRequested) return;
        if (DebugView) return;

        _jsInvocationQueue.Writer.TryWrite(new JSInvocation(
            identifier, cancellationToken, args, DateTime.UtcNow.Ticks
        ));
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

                    var elapsed = TimeSpan.FromTicks(
                        DateTime.UtcNow.Ticks - invocation.CreatedAtTicks
                    );
                    var remainingDelay = _liveViewJSInteropOptions
                        .DefaultDelayForInvocationForward
                        - elapsed;

                    if (remainingDelay > TimeSpan.Zero)
                    {
                        // A short delay so that components can finish rendering.
                        // For example when IJSRuntime.InvokeAsync is called in
                        // OnAfterRenderAsync.
                        await Task.Delay(remainingDelay, linkedCts.Token);
                    }

                    object?[]? newArgs = null;
                    if (invocation.Args is not null && invocation.Args.Length > 0)
                    {
                        newArgs = _dotnetToJsArgsTranslator.TranslateArgs(
                            invocation.Identifier, invocation.Args
                        );
                    }

                    var jsRuntime = Circuit.CircuitHost.JSRuntime;
                    await jsRuntime.Inner.InvokeVoidAsync(
                        invocation.Identifier,
                        linkedCts.Token,
                        newArgs
                    );
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                }
                catch (Exception ex)
                {
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

    public void NotifyWindowSizeSyncChanged(bool enabled)
    {
        if (WindowSizeSyncEnabled == enabled) return;
        WindowSizeSyncEnabled = enabled;
        WindowSizeSyncChanged?.Invoke(this);
    }

    public void NotifyScrollSyncChanged(bool enabled)
    {
        if (ScrollSyncEnabled == enabled) return;
        ScrollSyncEnabled = enabled;
        ScrollSyncChanged?.Invoke(this);
    }

    public void NotifyLaserPointerEnabledChanged(bool enabled)
    {
        if (LaserPointerEnabled == enabled) return;
        LaserPointerEnabled = enabled;
        PointerSyncChanged?.Invoke(this);
    }

    public void NotifyMirrorCursorChanged(Position? position)
    {
        if (position != null && CursorPosition != null)
        {
            if (CursorPosition.Value.x == position.Value.x &&
                CursorPosition.Value.y == position.Value.y)
                return;
        }
        CursorPosition = position;
        CursorPositionChanged?.Invoke(this);
    }

    public override void NotifyLoaded()
    {
        _pausedCircuitsTracker.MirrorCircuitLoaded(this);
    }
}

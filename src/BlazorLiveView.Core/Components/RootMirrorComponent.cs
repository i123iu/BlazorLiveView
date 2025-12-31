using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Root web component for mirror circuits. 
/// Contains one child <see cref="MirrorComponent"/>. 
/// </summary>
internal sealed class RootMirrorComponent(
    IOptions<LiveViewOptions> liveViewOptions
) : IComponent, IDisposable
{
    private struct Parameters
    {
        public IMirrorCircuit mirrorCircuit;
        public int ssrComponentId;
        public bool debugView;
    }

    private readonly LiveViewOptions _liveViewOptions = liveViewOptions.Value;
    private RenderHandle _renderHandle;
    private Parameters? _parameters = null;

    /// <summary>
    /// Initializes the parameters manually. 
    /// These parameters act as usual Blazor parameters. 
    /// </summary>
    internal void Initialize(IMirrorCircuit mirrorCircuit, int ssrComponentId, bool debugView)
    {
        if (_parameters != null)
            throw new InvalidOperationException("Already initialized");

        _parameters = new Parameters
        {
            mirrorCircuit = mirrorCircuit,
            ssrComponentId = ssrComponentId,
            debugView = debugView
        };

        mirrorCircuit.MirrorCircuitBlocked += OnMirrorCircuitBlocked;
        mirrorCircuit.SourceCircuit.CircuitStatusChanged += OnSourceCircuitStatusChanged;
    }

    public void Dispose()
    {
        if (_parameters is not null)
        {
            _parameters.Value.mirrorCircuit.MirrorCircuitBlocked -= OnMirrorCircuitBlocked;
            _parameters.Value.mirrorCircuit.SourceCircuit.CircuitStatusChanged -= OnSourceCircuitStatusChanged;
            _parameters = null;
        }
    }

    private void OnMirrorCircuitBlocked(IMirrorCircuit circuit)
    {
        if (_parameters is null)
            throw new InvalidOperationException();
        if (_parameters.Value.mirrorCircuit.Id != circuit.Id)
            throw new InvalidOperationException();

        _renderHandle.Dispatcher.InvokeAsync(Render);
    }

    private void OnSourceCircuitStatusChanged(ICircuit circuit)
    {
        if (_parameters is null)
            throw new InvalidOperationException();
        if (_parameters.Value.mirrorCircuit.SourceCircuit.Id != circuit.Id)
            throw new InvalidOperationException();

        _renderHandle.Dispatcher.InvokeAsync(Render);
    }

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
    }

    Task IComponent.SetParametersAsync(ParameterView parameters)
    {
        Render();
        return Task.CompletedTask;
    }

    private void Render()
    {
        if (!_renderHandle.IsInitialized || _parameters is null)
        {
            return;
        }

        Parameters parameters = _parameters.Value;

        if (parameters.mirrorCircuit.IsBlocked)
        {
            // This mirror circuit has been blocked - display the block message
            var blockReason = parameters.mirrorCircuit.BlockReason;
            RenderMessage(blockReason.message);
            return;
        }

        switch (parameters.mirrorCircuit.SourceCircuit.CircuitStatus)
        {
            case CircuitStatus.Open:
                RenderMessage("Target connection is starting...");
                break;

            case CircuitStatus.Up:
                var ssrComponentId = parameters.ssrComponentId;
                var sourceComponentId = parameters.mirrorCircuit.SourceCircuit
                    .SsrComponentIdToInteractiveComponentId(ssrComponentId);
                RenderMirrorComponent(
                    parameters.mirrorCircuit.SourceCircuit.Id,
                    sourceComponentId,
                    parameters.debugView
                );
                break;

            case CircuitStatus.Down:
                RenderMessage("Target connection is reconnecting...");
                break;

            case CircuitStatus.Closed:
                RenderMessage("Target connection is closed.");
                break;

            default: throw new NotImplementedException();
        }
    }

    private void RenderMirrorComponent(string sourceId, int sourceComponentId, bool debugView)
    {
        _renderHandle.Render(builder =>
        {
            builder.OpenComponent<MirrorComponent>(0);
            builder.AddAttribute(1, nameof(MirrorComponent.CircuitId), sourceId);
            builder.AddAttribute(2, nameof(MirrorComponent.ComponentId), sourceComponentId);
            builder.AddAttribute(3, nameof(MirrorComponent.DebugView), debugView);
            builder.CloseComponent();

            if (_liveViewOptions.UseScreenOverlay && !debugView)
            {
                builder.OpenElement(4, "div");
                builder.AddAttribute(5, "style",
                    """
                    position: fixed;
                    left: 0px;
                    right: 0px;
                    top: 0px;
                    bottom: 0px;
                    z-index: 1000000;
                    """);
                builder.CloseElement();
            }
        });
    }

    private void RenderMessage(string message)
    {
        _renderHandle.Render(builder =>
        {
            builder.AddContent(0, message);
        });
    }
}
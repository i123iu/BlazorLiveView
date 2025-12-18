using BlazorLiveView.Core.Circuits;
using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Root web component for mirror circuits. 
/// Contains one child <see cref="MirrorComponent"/>. 
/// </summary>
internal sealed class RootMirrorComponent : IComponent, IDisposable
{
    private struct Parameters
    {
        public IUserCircuit source;
        public int ssrComponentId;
        public bool debugView;
    }

    private RenderHandle _renderHandle;
    private Parameters? _parameters = null;

    /// <summary>
    /// Initializes the parameters manually. 
    /// These parameters act as usual Blazor parameters. 
    /// </summary>
    internal void Initialize(IUserCircuit source, int ssrComponentId, bool debugView)
    {
        _parameters = new Parameters
        {
            source = source,
            ssrComponentId = ssrComponentId,
            debugView = debugView
        };

        source.CircuitStatusChanged += OnCircuitStatusChanged;
    }

    public void Dispose()
    {
        if (_parameters is not null)
        {
            _parameters.Value.source.CircuitStatusChanged -= OnCircuitStatusChanged;
            _parameters = null;
        }
    }

    private void OnCircuitStatusChanged(ICircuit circuit)
    {
        if (_parameters is null) throw new InvalidOperationException();
        if (!ReferenceEquals(_parameters.Value.source, circuit)) throw new InvalidOperationException();
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

        switch (_parameters.Value.source.CircuitStatus)
        {
            case CircuitStatus.Open:
                RenderMessage("Target connection is starting...");
                break;

            case CircuitStatus.Up:
                var ssrComponentId = _parameters.Value.ssrComponentId;
                var sourceComponentId = _parameters.Value.source
                    .SsrComponentIdToInteractiveComponentId(ssrComponentId);
                RenderMirrorComponent(
                    _parameters.Value.source.Id,
                    sourceComponentId,
                    _parameters.Value.debugView
                );
                break;

            case CircuitStatus.Down:
                RenderMessage("Target connection is reconnecting...");
                break;

            case CircuitStatus.Closed:
                RenderMessage("Target connection is closed.");
                break;
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
using BlazorLiveView.Core.Circuits;
using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Root web component for mirror circuits. 
/// Contains one child <see cref="MirrorComponent"/>. 
/// </summary>
internal sealed class RootMirrorComponent : IComponent
{
    private RenderHandle _renderHandle;

    private IRegularCircuit _source = null!;
    private int _ssrComponentId = 0;

    /// <summary>
    /// Initializes the parameters manually. 
    /// These parameters act as usual Blazor parameters. 
    /// </summary>
    internal void Initialize(IRegularCircuit source, int ssrComponentId)
    {
        _source = source;
        _ssrComponentId = ssrComponentId;
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
        if (!_renderHandle.IsInitialized)
        {
            return;
        }

        if (_source is null)
        {
            _renderHandle.Render(builder =>
            {
                builder.AddContent(0, "-");
            });
            return;
        }

        var sourceComponentId = _source.SsrComponentIdToInteractiveComponentId(_ssrComponentId);

        _renderHandle.Render(builder =>
        {
            builder.OpenComponent<MirrorComponent>(0);
            builder.AddAttribute(1, nameof(MirrorComponent.CircuitId), _source.Id);
            builder.AddAttribute(2, nameof(MirrorComponent.ComponentId), sourceComponentId);
            builder.AddAttribute(3, nameof(MirrorComponent.DebugView), false);
            builder.CloseComponent();
        });
    }
}
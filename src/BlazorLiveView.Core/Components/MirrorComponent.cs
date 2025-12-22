using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection.Wrappers;
using BlazorLiveView.Core.RenderTree;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Clones the render tree of another component, while translating child
/// components of the original tree to <see cref="MirrorComponent"/> instances.
/// </summary>
internal sealed class MirrorComponent(
    ICircuitTracker circuitTracker,
    IRenderTreeTranslatorFactory translatorFactory,
    ILogger<MirrorComponent> logger
) : IComponent
{
    public const int NO_PARAMETERS = 3;

    private readonly ICircuitTracker _circuitTracker = circuitTracker;
    private readonly IRenderTreeTranslatorFactory _translatorFactory = translatorFactory;
    private readonly ILogger<MirrorComponent> _logger = logger;
    private RenderHandle _renderHandle;

    [Parameter]
    public string CircuitId { get; set; } = "";

    [Parameter]
    public int ComponentId { get; set; } = -1;

    [Parameter]
    public bool DebugView { get; set; } = false;

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
    }

    Task IComponent.SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (string.IsNullOrEmpty(CircuitId))
            throw new ArgumentException($"Parameter '{nameof(CircuitId)}' is required.");
        if (ComponentId == -1)
            throw new ArgumentException($"Parameter '{nameof(ComponentId)}' is required.");

        var circuit = _circuitTracker.GetCircuit(CircuitId)
            ?? throw new ArgumentException($"Circuit with ID '{CircuitId}' not found.");
        if (circuit is not IUserCircuit userCircuit)
        {
            throw new ArgumentException($"Cannot mirror a mirror circuit. Circuit ID: '{CircuitId}'.");
        }
        userCircuit.ComponentRerendered += OriginalComponentRerendered;

        Render();
        return Task.CompletedTask;
    }

    private void OriginalComponentRerendered(IUserCircuit circuit, int componentId)
    {
        if (circuit.Id != CircuitId)
        {
            throw new InvalidOperationException(
                $"Circuit ID mismatch. Expected '{CircuitId}', got '{circuit.Id}'."
            );
        }

        if (componentId == ComponentId)
        {
            _renderHandle.Dispatcher.InvokeAsync(Render);
        }
    }

    private void Render()
    {
        var circuit = _circuitTracker.GetCircuit(CircuitId)
            ?? throw new InvalidOperationException($"Circuit with ID '{CircuitId}' not found.");

        if (circuit is not IUserCircuit userCircuit)
        {
            throw new InvalidOperationException(
                $"Cannot mirror a mirror circuit. Circuit ID: '{CircuitId}'."
            );
        }

        var componentState = userCircuit.GetOptionalComponentState(ComponentId);
        if (componentState is null)
        {
            _logger.LogWarning("Component with ID '{ComponentId}' not found in circuit '{CircuitId}'.",
                ComponentId, CircuitId);
            _renderHandle.Render(builder => { });
            return;
        }

        Render(componentState);
    }

    private void Render(ComponentState componentState)
    {
        ComponentStateWrapper componentStateWrapper =
            new(componentState);
        var sourceBuilder = componentStateWrapper.CurrentRenderTree;
        var frames = sourceBuilder.GetFrames().Array.AsSpan();
        var view = BuildView(
            componentState,
            frames
        );

        _renderHandle.Render(builder =>
        {
            RenderTreeBuilderWrapper builderWrapper = new(builder);
            builderWrapper.Entries.Append(view);
        });
    }

    private readonly List<RenderTreeFrame> _builder = new(capacity: 1024);
    private RenderTreeFrame[] BuildView(
        ComponentState componentState,
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        _builder.Clear();
        _builder.EnsureCapacity(frames.Length + 20);

        var translator = DebugView
            ? _translatorFactory.CreateDebugTranslator(
                _builder,
                CircuitId,
                componentState.Component.GetType(),
                ComponentId
            )
            : _translatorFactory.CreateMirrorTranslator(
                _builder,
                CircuitId
            );

        translator.TranslateRoot(frames);

        string log = _builder.Count == 0
            ? "<empty>"
            : string.Join("\n", _builder.Select(frame => $"- {frame}"));
        _logger.LogTrace(
            "Translated {CircuitId}{ComponentId} to: \n{Frames}",
            CircuitId,
            ComponentId,
            log
        );

        var view = _builder.ToArray();
        _builder.Clear();

        return view;
    }
}
using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection.Wrappers;
using BlazorLiveView.Core.RenderTree;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Clones the render tree of another component. 
/// Translates child components in the original tree to instances of <see cref="MirrorComponent"/>. 
/// </summary>
internal sealed class MirrorComponent(
    ICircuitTracker circuitTracker,
    IRenderTreeMirrorTranslatorFactory translatorFactory,
    ILogger<MirrorComponent> logger
) : IComponent
{
    private readonly ICircuitTracker _circuitTracker = circuitTracker;
    private readonly IRenderTreeMirrorTranslatorFactory _translatorFactory = translatorFactory;
    private readonly ILogger<MirrorComponent> _logger = logger;
    private RenderHandle _renderHandle;

    [Parameter]
    public string CircuitId { get; set; } = "";

    [Parameter]
    public int ComponentId { get; set; } = 0;

    [Parameter]
    public bool DebugView { get; set; } = true;

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
        Render();
    }

    Task IComponent.SetParametersAsync(ParameterView parameters)
    {
        if (!parameters.TryGetValue<string>(nameof(CircuitId), out string? circuitId))
            throw new ArgumentException($"Parameter '{nameof(CircuitId)}' is required.");
        if (!parameters.TryGetValue(nameof(ComponentId), out int componentId))
            throw new ArgumentException($"Parameter '{nameof(ComponentId)}' is required.");
        if (!parameters.TryGetValue(nameof(DebugView), out bool debugView))
            debugView = true;

        CircuitId = circuitId;
        ComponentId = componentId;
        DebugView = debugView;

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
        var circuit = _circuitTracker.GetCircuit(CircuitId);
        if (circuit is null)
        {
            _logger.LogWarning(
                "Circuit with ID '{CircuitId}' not found.",
                CircuitId
            );
            _renderHandle.Render(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, $"No circuit");
                builder.CloseElement();
            });
            return;
        }

        if (circuit is not IUserCircuit userCircuit)
        {
            _logger.LogWarning(
                "Circuit with ID '{CircuitId}' is not a user circuit.",
                CircuitId
            );
            _renderHandle.Render(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, $"Not user circuit");
                builder.CloseElement();
            });
            return;
        }

        var componentState = userCircuit.GetComponentState(ComponentId);
        if (componentState is null)
        {
            _logger.LogWarning(
                "Component with ID '{ComponentId}' not found in circuit '{CircuitId}'.",
                ComponentId,
                CircuitId
            );
            _renderHandle.Render(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, $"No component state");
                builder.CloseElement();
            });
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

        if (DebugView)
        {
            BuildDebugView(componentState, frames, _builder);
        }
        else
        {
            BuildUserView(frames, _builder);
        }

        var view = _builder.ToArray();
        _builder.Clear();

        return view;
    }

    private void BuildUserView(
        ReadOnlySpan<RenderTreeFrame> frames,
        List<RenderTreeFrame> result
    )
    {
        var translator = _translatorFactory.CreateTranslator(
            result,
            CircuitId
        );
        translator.TranslateAll(frames);

        string log = result.Count == 0
            ? "<empty>"
            : string.Join("\n", result.Select(frame => $"- {frame}"));
        _logger.LogInformation(
            "Translated {CircuitId}{ComponentId} to: \n{Frames}",
            CircuitId,
            ComponentId,
            log
        );
    }

    private void BuildDebugView(
        ComponentState componentState,
        ReadOnlySpan<RenderTreeFrame> frames,
        List<RenderTreeFrame> result
    )
    {
        result.Add(RenderTreeFrameBuilder.Element(
            0,
            0, // Updated later
            "div"
        ));
        result.Add(RenderTreeFrameBuilder.Attribute(
            1,
            "style",
            "border: 1px solid black;"
        ));
        result.Add(RenderTreeFrameBuilder.Text(
            2,
            $"DEBUG VIEW OF '{componentState.Component.GetType().Name}'" +
            $" componentId={ComponentId}"
        ));

        for (int i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];

            int startIndex = result.Count;
            result.Add(RenderTreeFrameBuilder.Element(
                3,
                0, // Updated later
                "div"
            ));

            switch (frame.FrameType)
            {
                case RenderTreeFrameType.None:
                    // Skip
                    break;

                case RenderTreeFrameType.Component:
                    result.Add(RenderTreeFrameBuilder.Text(
                        4,
                        $"{i}: {frame}, componentId={frame.ComponentId}"
                    ));
                    break;

                default:
                    result.Add(RenderTreeFrameBuilder.Text(
                        4,
                        $"{i}: {frame}"
                    ));
                    break;
            }

            result[startIndex] = RenderTreeFrameBuilder.Element(
                3,
                result.Count - startIndex,
                "div"
            );
        }

        result[0] = RenderTreeFrameBuilder.Element(
            0,
            result.Count,
            "div"
        );
    }
}
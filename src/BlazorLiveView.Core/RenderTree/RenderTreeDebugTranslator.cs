using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeDebugTranslator(
    List<RenderTreeFrame> result,
    string circuitId,
    Type componentType,
    int componentId
) : RenderTreeTranslatorBase(
    result
)
{
    private readonly string _circuitId = circuitId;

    private int index = 0;
    private int indent = 0;
    private readonly List<int> childComponentIds = new();

    public override void TranslateRoot(ReadOnlySpan<RenderTreeFrame> frames)
    {
        int seq = 0;

        // <div ...>
        using SubtreeLengthCapture rootDiv = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                seq++,
                subtreeLength,
                "div"
            )
        );

        _result.Add(RenderTreeFrameBuilder.Attribute(
            seq++,
            "style",
            "border: 1px solid black; padding: 4px; margin: 4px; "
        ));

        // Component name
        using (SubtreeLengthCapture nameDiv = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                seq++, subtreeLength, "div"
            )
        ))
        {
            _result.Add(RenderTreeFrameBuilder.Attribute(
                seq++,
                "style",
                "font-weight: bold; padding-left: 8px; "
            ));
            _result.Add(RenderTreeFrameBuilder.Text(
                seq++,
                $"Component with id={componentId} type='{componentType.Name}': "
            ));
        }

        // Frames
        base.TranslateRoot(frames);

        // Add all child components
        foreach (int childComponentId in childComponentIds)
        {
            AddMirrorComponent(
                index * SEQ_MUL,
                null,
                _circuitId,
                childComponentId,
                true
            );
        }
    }

    private void AddFrameAsText(RenderTreeFrame frame)
    {
        using SubtreeLengthCapture div = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                frame.Sequence * SEQ_MUL + 0,
                subtreeLength,
                "div"
            )
        );

        _result.Add(RenderTreeFrameBuilder.Attribute(
            frame.Sequence * SEQ_MUL + 1,
            "style",
            $"padding-left: {indent}em; "
        ));

        switch (frame.FrameType)
        {
            case RenderTreeFrameType.None:
                // Skip
                return;

            case RenderTreeFrameType.Component:
                _result.Add(RenderTreeFrameBuilder.Text(
                    frame.Sequence * SEQ_MUL + 2,
                    $"{index}: {frame}, componentId={frame.ComponentId}"
                ));
                break;

            default:
                _result.Add(RenderTreeFrameBuilder.Text(
                    frame.Sequence * SEQ_MUL + 2,
                    $"{index}: {frame}"
                ));
                break;
        }

        index++;
    }

    private void AddFramesAsText(ReadOnlySpan<RenderTreeFrame> frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            AddFrameAsText(frames[i]);
        }
    }

    protected override void TranslateNone(RenderTreeFrame none)
    {
        // Skip
    }

    protected override void TranslateElement(
        RenderTreeFrame element,
        ReadOnlySpan<RenderTreeFrame> attributes,
        ReadOnlySpan<RenderTreeFrame> childFrames
    )
    {
        AddFrameAsText(element);
        indent++;
        AddFramesAsText(attributes);
        TranslateAll(childFrames);
        indent--;
    }

    protected override void TranslateText(RenderTreeFrame text)
    {
        AddFrameAsText(text);
    }

    protected override void TranslateComponent(
        RenderTreeFrame component,
        ReadOnlySpan<RenderTreeFrame> attributes
    )
    {
        var id = component.ComponentId;
        if (componentId != id)
        {
            childComponentIds.Add(id);
        }

        AddFrameAsText(component);
        indent++;
        AddFramesAsText(attributes);
        indent--;
    }

    protected override void TranslateRegion(
        RenderTreeFrame region,
        ReadOnlySpan<RenderTreeFrame> childFrames
    )
    {
        AddFrameAsText(region);
        indent++;
        TranslateAll(childFrames);
        indent--;
    }

    protected override void TranslateMarkup(RenderTreeFrame markup)
    {
        AddFrameAsText(markup);
    }
}

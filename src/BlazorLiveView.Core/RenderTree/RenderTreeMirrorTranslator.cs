using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeMirrorTranslator(
    List<RenderTreeFrame> result,
    string circuitId
) : RenderTreeMirrorTranslatorBase(
    result
)
{
    private readonly string _circuitId = circuitId;

    private void TranslateAttributes(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        foreach (var frame in frames)
        {
            if (frame.FrameType != RenderTreeFrameType.Attribute)
                throw new ArgumentException("Expected only attribute frames.", nameof(frames));

            _result.Add(RenderTreeFrameBuilder.Attribute(
                frame.Sequence * SEQ_MUL,
                frame.AttributeName,
                frame.AttributeValue,
                frame.AttributeEventHandlerId,
                frame.AttributeEventUpdatesAttributeName
            ));
        }
    }

    protected override void TranslateElement(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var element = AssertFirstFrame(frames, RenderTreeFrameType.Element);
        AssertEqualSubtreeLength(frames, element.ElementSubtreeLength);

        int newElementIndex = StartCaptureSubtreeLength();

        var attributes = TakeAttributes(frames[1..]);
        TranslateAttributes(attributes);

        TranslateAll(frames[(1 + attributes.Length)..]);

        StopCaptureSubtreeLength(newElementIndex, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                element.Sequence * SEQ_MUL,
                subtreeLength,
                element.ElementName,
                element.ElementKey
            )
        );
    }

    protected override void TranslateComponent(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var component = AssertFirstFrame(frames, RenderTreeFrameType.Component);
        AssertEqualSubtreeLength(frames, component.ComponentSubtreeLength);

        var originalAttributes = TakeAttributes(frames[1..]);
        if (originalAttributes.Length != frames.Length - 1)
        {
            throw new Exception("Component contains more than just attributes");
        }

        AddMirrorComponent(
            component.Sequence * SEQ_MUL,
            component.ComponentKey,
            _circuitId,
            component.ComponentId,
            false
        );
    }

    protected override void TranslateRegion(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var region = AssertFirstFrame(frames, RenderTreeFrameType.Region);
        AssertEqualSubtreeLength(frames, region.RegionSubtreeLength);

        if (frames.Length > 2 && frames[1].FrameType == RenderTreeFrameType.Attribute)
        {
            throw new Exception("Region first frame should not be an attribute");
        }

        int newRegionIndex = StartCaptureSubtreeLength();

        TranslateAll(frames[1..]);

        StopCaptureSubtreeLength(newRegionIndex, subtreeLength =>
            RenderTreeFrameBuilder.Region(
                region.Sequence * SEQ_MUL,
                subtreeLength
            )
        );
    }

    protected override void TranslateText(
        RenderTreeFrame frame
    )
    {
        if (frame.FrameType != RenderTreeFrameType.Text)
            throw new ArgumentException($"Expected frame {frame.FrameType} to be of type Text.", nameof(frame));

        _result.Add(RenderTreeFrameBuilder.Text(
            frame.Sequence * SEQ_MUL,
            frame.TextContent
        ));
    }

    protected override void TranslateMarkup(RenderTreeFrame frame)
    {
        if (frame.FrameType != RenderTreeFrameType.Markup)
            throw new ArgumentException($"Expected frame {frame.FrameType} to be of type Markup.", nameof(frame));

        _result.Add(RenderTreeFrameBuilder.Markup(
            frame.Sequence * SEQ_MUL,
            frame.MarkupContent
        ));
    }

    private static ReadOnlySpan<RenderTreeFrame> TakeAttributes(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        int count;
        for (count = 0; count < frames.Length; count++)
        {
            if (frames[count].FrameType != RenderTreeFrameType.Attribute)
                break;
        }
        return frames[..count];
    }
}

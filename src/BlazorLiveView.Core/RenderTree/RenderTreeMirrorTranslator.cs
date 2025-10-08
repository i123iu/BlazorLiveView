using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeMirrorTranslator(
    List<RenderTreeFrame> result,
    string circuitId,
    ILogger<RenderTreeMirrorTranslator> logger
) : IRenderTreeMirrorTranslator
{
    private const int SEQ_MUL = 10;

    private readonly List<RenderTreeFrame> _result = result;
    private readonly string _circuitId = circuitId;
    private readonly ILogger<RenderTreeMirrorTranslator> _logger = logger;

    public void TranslateAll(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        for (int i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.None:
                    // Skip
                    break;

                case RenderTreeFrameType.Element:
                    var elementSubtreeLength = frame.ElementSubtreeLength;
                    var elementFrames = frames.Slice(i, elementSubtreeLength);
                    TranslateElement(
                        elementFrames
                    );
                    i += elementSubtreeLength - 1;
                    break;

                case RenderTreeFrameType.Text:
                    _result.Add(RenderTreeFrameBuilder.Text(
                        frame.Sequence * SEQ_MUL,
                        frame.TextContent
                    ));
                    break;

                case RenderTreeFrameType.Attribute:
                    throw new Exception("Attribute without parent");

                case RenderTreeFrameType.Component:
                    var componentSubtreeLength = frame.ComponentSubtreeLength;
                    var componentFrames = frames.Slice(i, componentSubtreeLength);
                    TranslateComponent(
                        componentFrames
                    );
                    i += componentSubtreeLength - 1;
                    break;

                case RenderTreeFrameType.Region:
                    var regionSubtreeLength = frame.RegionSubtreeLength;
                    var regionFrames = frames.Slice(i, regionSubtreeLength);
                    TranslateRegion(
                        regionFrames
                    );
                    i += regionSubtreeLength - 1;
                    break;

                case RenderTreeFrameType.ElementReferenceCapture:
                    throw new NotImplementedException();

                case RenderTreeFrameType.ComponentReferenceCapture:
                    throw new NotImplementedException();

                case RenderTreeFrameType.Markup:
                    _result.Add(RenderTreeFrameBuilder.Markup(
                        frame.Sequence * SEQ_MUL,
                        frame.MarkupContent
                    ));
                    break;

                case RenderTreeFrameType.ComponentRenderMode:
                    throw new NotImplementedException();

                case RenderTreeFrameType.NamedEvent:
                    throw new NotImplementedException();

                default: throw new NotImplementedException();
            }
        }
    }

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

    private void TranslateElement(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var element = CheckFirstFrame(frames, RenderTreeFrameType.Element);
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

    private void TranslateComponent(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var component = CheckFirstFrame(frames, RenderTreeFrameType.Component);
        AssertEqualSubtreeLength(frames, component.ComponentSubtreeLength);

        var originalAttributes = TakeAttributes(frames[1..]);
        if (originalAttributes.Length != frames.Length - 1)
        {
            throw new Exception("Component contains more than just attributes");
        }

        _result.Add(RenderTreeFrameBuilder.Component(
            component.Sequence * SEQ_MUL + 0,
            4,
            typeof(MirrorComponent),
            null,
            component.ComponentKey
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            component.Sequence * SEQ_MUL + 1,
            nameof(MirrorComponent.CircuitId),
            _circuitId
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            component.Sequence * SEQ_MUL + 2,
            nameof(MirrorComponent.ComponentId),
            component.ComponentId
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            component.Sequence * SEQ_MUL + 3,
            nameof(MirrorComponent.DebugView),
            false
        ));
    }

    private void TranslateRegion(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        var region = CheckFirstFrame(frames, RenderTreeFrameType.Region);
        AssertEqualSubtreeLength(frames, region.RegionSubtreeLength);

        if (frames.Length > 2)
        {
            // check only first
            if (frames[1].FrameType == RenderTreeFrameType.Attribute)
            {
                throw new Exception("NRIWVEgfds");
            }
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

    private int StartCaptureSubtreeLength()
    {
        int rootIndex = _result.Count;
        _result.Add(RenderTreeFrameBuilder.None());
        return rootIndex;
    }

    private void StopCaptureSubtreeLength(int rootIndex, Func<int, RenderTreeFrame> frameBuilder)
    {
        int subtreeLength = _result.Count - rootIndex;

        var frame = _result[rootIndex];
        if (frame.FrameType != RenderTreeFrameType.None)
            throw new InvalidOperationException("Invalid state");

        _result[rootIndex] = frameBuilder(subtreeLength);
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

    private static RenderTreeFrame CheckFirstFrame(
        ReadOnlySpan<RenderTreeFrame> frames,
        RenderTreeFrameType type
    )
    {
        if (frames.IsEmpty)
            throw new ArgumentException("No frames to translate.", nameof(frames));

        var first = frames[0];
        if (frames[0].FrameType != type)
            throw new ArgumentException($"Expected first frame to be of type {type}.", nameof(frames));

        return first;
    }

    private static void AssertEqualSubtreeLength(
        ReadOnlySpan<RenderTreeFrame> frames,
        int expected
    )
    {
        if (frames.Length != expected)
        {
            throw new ArgumentException($"");
        }
    }
}

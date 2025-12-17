using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal abstract class RenderTreeMirrorTranslatorBase(
    List<RenderTreeFrame> result
) : IRenderTreeMirrorTranslator
{
    internal const int SEQ_MUL = 10;

    protected readonly List<RenderTreeFrame> _result = result;

    public virtual void TranslateAll(
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
                    TranslateText(frame);
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
                    TranslateMarkup(frame);
                    break;

                case RenderTreeFrameType.ComponentRenderMode:
                    throw new NotImplementedException();

                case RenderTreeFrameType.NamedEvent:
                    throw new NotImplementedException();

                default: throw new NotImplementedException();
            }
        }
    }

    protected abstract void TranslateElement(
        ReadOnlySpan<RenderTreeFrame> frames
    );

    protected abstract void TranslateComponent(
        ReadOnlySpan<RenderTreeFrame> frames
    );

    protected abstract void TranslateRegion(
        ReadOnlySpan<RenderTreeFrame> frames
    );

    protected abstract void TranslateText(
        RenderTreeFrame frame
    );

    protected abstract void TranslateMarkup(
        RenderTreeFrame frame
    );

    protected void AddMirrorComponent(
        int firstSeq,
        object? componentKey,
        string circuitId,
        int componentId,
        bool debugView
    )
    {
        _result.Add(RenderTreeFrameBuilder.Component(
            firstSeq + 0,
            4,
            typeof(MirrorComponent),
            null,
            componentKey
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 1,
            nameof(MirrorComponent.CircuitId),
            circuitId
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 2,
            nameof(MirrorComponent.ComponentId),
            componentId
        ));
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 3,
            nameof(MirrorComponent.DebugView),
            debugView
        ));
    }

    protected int StartCaptureSubtreeLength()
    {
        int rootIndex = _result.Count;
        _result.Add(RenderTreeFrameBuilder.None());
        return rootIndex;
    }

    protected void StopCaptureSubtreeLength(int rootIndex, Func<int, RenderTreeFrame> frameBuilder)
    {
        int subtreeLength = _result.Count - rootIndex;

        var frame = _result[rootIndex];
        if (frame.FrameType != RenderTreeFrameType.None)
            throw new InvalidOperationException("Invalid state");

        _result[rootIndex] = frameBuilder(subtreeLength);
    }

    protected static RenderTreeFrame AssertFirstFrame(
        ReadOnlySpan<RenderTreeFrame> frames,
        RenderTreeFrameType type
    )
    {
        if (frames.IsEmpty)
            throw new ArgumentException("No frames to translate.", nameof(frames));

        var first = frames[0];
        if (first.FrameType != type)
            throw new ArgumentException($"Expected first frame {first.FrameType} to be of type {type}.", nameof(frames));

        return first;
    }

    protected static void AssertEqualSubtreeLength(
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

using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal abstract class RenderTreeTranslatorBase(
    List<RenderTreeFrame> result
) : IRenderTreeTranslator
{
    internal const int SEQ_MUL = MirrorComponent.NO_PARAMETERS + 1;

    protected readonly List<RenderTreeFrame> _result = result;

    public virtual void TranslateRoot(ReadOnlySpan<RenderTreeFrame> frames)
    {
        TranslateAll(frames);
    }

    protected virtual void TranslateAll(ReadOnlySpan<RenderTreeFrame> frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.None:
                    TranslateNone(frame);
                    break;

                case RenderTreeFrameType.Element:
                    {
                        var subtreeLength = frame.ElementSubtreeLength;
                        var subtreeFrames = frames.Slice(i + 1, subtreeLength - 1);

                        var attributes = TakeAttributes(subtreeFrames);
                        var childFrames = subtreeFrames[attributes.Length..];

                        TranslateElement(
                            frame,
                            attributes,
                            childFrames
                        );

                        i += subtreeLength - 1;
                    }
                    break;

                case RenderTreeFrameType.Text:
                    TranslateText(frame);
                    break;

                case RenderTreeFrameType.Attribute:
                    throw new RenderTreeTranslationException("Attribute without parent");

                case RenderTreeFrameType.Component:
                    {
                        var subtreeLength = frame.ComponentSubtreeLength;
                        var subtreeFrames = frames.Slice(i + 1, subtreeLength - 1);

                        for (int j = 0; j < subtreeFrames.Length; j++)
                        {
                            if (subtreeFrames[j].FrameType != RenderTreeFrameType.Attribute)
                            {
                                throw new RenderTreeTranslationException("Component contains more than just attributes");
                            }
                        }

                        TranslateComponent(
                            frame,
                            subtreeFrames
                        );

                        i += subtreeLength - 1;
                    }
                    break;

                case RenderTreeFrameType.Region:
                    {
                        var subtreeLength = frame.RegionSubtreeLength;
                        var subtreeFrames = frames.Slice(i + 1, subtreeLength - 1);

                        TranslateRegion(
                            frame,
                            subtreeFrames
                        );

                        i += subtreeLength - 1;
                    }
                    break;

                case RenderTreeFrameType.ElementReferenceCapture:
                    throw new RenderTreeTranslationException("Not implemented");

                case RenderTreeFrameType.ComponentReferenceCapture:
                    throw new RenderTreeTranslationException("Not implemented");

                case RenderTreeFrameType.Markup:
                    TranslateMarkup(frame);
                    break;

                case RenderTreeFrameType.ComponentRenderMode:
                    throw new RenderTreeTranslationException("Not implemented");

                case RenderTreeFrameType.NamedEvent:
                    throw new RenderTreeTranslationException("Not implemented");

                default: throw new RenderTreeTranslationException("Not implemented");
            }
        }
    }

    protected abstract void TranslateNone(
        RenderTreeFrame none
    );

    protected abstract void TranslateElement(
        RenderTreeFrame element,
        ReadOnlySpan<RenderTreeFrame> attributes,
        ReadOnlySpan<RenderTreeFrame> childFrames
    );

    protected abstract void TranslateText(
        RenderTreeFrame text
    );

    protected abstract void TranslateComponent(
        RenderTreeFrame component,
        ReadOnlySpan<RenderTreeFrame> attributes
    );

    protected abstract void TranslateRegion(
        RenderTreeFrame region,
        ReadOnlySpan<RenderTreeFrame> childFrames
    );

    protected abstract void TranslateMarkup(
        RenderTreeFrame markup
    );

    /// <summary>
    /// Takes and returns all attributes from the beginning of <paramref name="frames"/>. 
    /// </summary>
    private static ReadOnlySpan<RenderTreeFrame> TakeAttributes(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        int count = 0;
        while (count < frames.Length)
        {
            if (frames[count].FrameType != RenderTreeFrameType.Attribute)
                break;
            count++;
        }
        return frames[..count];
    }

    protected void AddMirrorComponent(
        int firstSeq,
        object? componentKey,
        string circuitId,
        int componentId,
        bool debugView
    )
    {
        // <MirrorCimponent 
        SubtreeLengthCapture mirrorComponent = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Component(
                firstSeq + 0,
                subtreeLength,
                typeof(MirrorComponent),
                null,
                componentKey
            )
        );

        // CircuitId="@circuitId"
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 1,
            nameof(MirrorComponent.CircuitId),
            circuitId
        ));

        // ComponentId="@componentId"
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 2,
            nameof(MirrorComponent.ComponentId),
            componentId
        ));

        // DebugView="@debugView"
        _result.Add(RenderTreeFrameBuilder.Attribute(
            firstSeq + 3,
            nameof(MirrorComponent.DebugView),
            debugView
        ));

        // />
        mirrorComponent.Dispose();
    }

    protected readonly struct SubtreeLengthCapture : IDisposable
    {
        public delegate RenderTreeFrame BuildRenderTreeFrameHandler(int subtreeLength);

        public SubtreeLengthCapture(
            IList<RenderTreeFrame> result,
            BuildRenderTreeFrameHandler buildRenderTreeFrame
        )
        {
            this.buildRenderTreeFrame = buildRenderTreeFrame;
            this.result = result;

            rootIndex = this.result.Count;
            this.result.Add(RenderTreeFrameBuilder.None());
        }

        private readonly int rootIndex;
        private readonly BuildRenderTreeFrameHandler buildRenderTreeFrame;
        private readonly IList<RenderTreeFrame> result;

        public void Dispose()
        {
            int subtreeLength = result.Count - rootIndex;

            var frame = result[rootIndex];
            if (frame.FrameType != RenderTreeFrameType.None)
                throw new RenderTreeTranslationException($"Invalid state of {nameof(SubtreeLengthCapture)}");

            result[rootIndex] = buildRenderTreeFrame(subtreeLength);
        }
    }
}

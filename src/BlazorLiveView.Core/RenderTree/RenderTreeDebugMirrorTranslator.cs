using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeDebugMirrorTranslator(
    List<RenderTreeFrame> result,
    string circuitId,
    Type componentType,
    int componentId
) : RenderTreeMirrorTranslatorBase(
    result
)
{
    private readonly string _circuitId = circuitId;

    private int index;
    private readonly List<int> childComponentIds = new();

    public override void TranslateAll(ReadOnlySpan<RenderTreeFrame> frames)
    {
        int seq = 0;

        // <div ...>
        int rootIndex = StartCaptureSubtreeLength();
        seq++;

        _result.Add(RenderTreeFrameBuilder.Attribute(
            seq++,
            "style",
            "border: 1px solid black; padding: 4px; margin: 4px; "
        ));
        {
            int compNameDivIndex = StartCaptureSubtreeLength();
            int compNameDivSeq = seq++;
            _result.Add(RenderTreeFrameBuilder.Attribute(
                seq++,
                "style",
                "font-weight: bold; padding-left: 8px; "
            ));
            _result.Add(RenderTreeFrameBuilder.Text(
                seq++,
                $"DEBUG VIEW OF '{componentType.Name}'" +
                $" componentId={componentId}"
            ));
            StopCaptureSubtreeLength(compNameDivIndex, subtreeLength =>
                RenderTreeFrameBuilder.Element(
                    compNameDivSeq,
                    subtreeLength,
                    "div"
                )
            );
        }

        base.TranslateAll(frames);

        // </div>
        StopCaptureSubtreeLength(rootIndex, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                0,
                subtreeLength,
                "div"
            )
        );

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
        switch (frame.FrameType)
        {
            case RenderTreeFrameType.None:
                // Skip
                return;

            case RenderTreeFrameType.Component:
                _result.Add(RenderTreeFrameBuilder.Text(
                    frame.Sequence * SEQ_MUL,
                    $"{index}: {frame}, componentId={frame.ComponentId}"
                ));
                break;

            default:
                _result.Add(RenderTreeFrameBuilder.Text(
                    frame.Sequence * SEQ_MUL,
                    $"{index}: {frame}"
                ));
                break;
        }

        _result.Add(RenderTreeFrameBuilder.Element(
            frame.Sequence * SEQ_MUL + 1,
            1,
            "br"
        ));

        index++;
    }

    private void AddFramesAsText(ReadOnlySpan<RenderTreeFrame> frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].FrameType == RenderTreeFrameType.Component
                && frames[i].ComponentId != componentId)
            {
                childComponentIds.Add(frames[i].ComponentId);
            }
            AddFrameAsText(frames[i]);
        }
    }

    protected override void TranslateComponent(
        ReadOnlySpan<RenderTreeFrame> frames
    )
    {
        AddFramesAsText(frames);
    }

    protected override void TranslateElement(ReadOnlySpan<RenderTreeFrame> frames)
    {
        AddFramesAsText(frames);
    }

    protected override void TranslateRegion(ReadOnlySpan<RenderTreeFrame> frames)
    {
        AddFramesAsText(frames);
    }

    protected override void TranslateText(RenderTreeFrame frame)
    {
        AddFrameAsText(frame);
    }

    protected override void TranslateMarkup(RenderTreeFrame frame)
    {
        AddFrameAsText(frame);
    }
}

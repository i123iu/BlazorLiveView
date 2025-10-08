using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeMirrorTranslator
{
    void TranslateAll(ReadOnlySpan<RenderTreeFrame> frames);
}

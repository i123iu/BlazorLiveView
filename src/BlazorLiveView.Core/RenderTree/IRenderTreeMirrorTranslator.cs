using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeMirrorTranslator
{
    /// <summary>
    /// Replaces all component frames with <see cref="MirrorComponent"/> frames and adds the necessary parameters.
    /// </summary>
    void TranslateAll(ReadOnlySpan<RenderTreeFrame> frames);
}

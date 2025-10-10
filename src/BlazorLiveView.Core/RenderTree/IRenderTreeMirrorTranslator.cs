using Microsoft.AspNetCore.Components.RenderTree;
using BlazorLiveView.Core.Components;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeMirrorTranslator
{
    /// <summary>
    /// Translates the given frames (of an original component) into frames
    /// for a <see cref="MirrorComponent"/>. 
    /// </summary>
    void TranslateAll(ReadOnlySpan<RenderTreeFrame> frames);
}

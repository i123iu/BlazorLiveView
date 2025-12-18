using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeTranslator
{
    /// <summary>
    /// Translates the given frames (of an original component) into frames
    /// for a <see cref="MirrorComponent"/>. 
    /// </summary>
    void TranslateRoot(ReadOnlySpan<RenderTreeFrame> frames);
}

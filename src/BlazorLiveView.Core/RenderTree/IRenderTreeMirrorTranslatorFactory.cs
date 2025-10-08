using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeMirrorTranslatorFactory
{
    IRenderTreeMirrorTranslator CreateTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    );
}

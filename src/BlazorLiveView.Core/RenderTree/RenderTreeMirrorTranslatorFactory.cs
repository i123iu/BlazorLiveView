 using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeMirrorTranslatorFactory
    : IRenderTreeMirrorTranslatorFactory
{
    public IRenderTreeMirrorTranslator CreateTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    )
    {
        return new RenderTreeMirrorTranslator(
            result,
            circuitId
        );
    }

    public IRenderTreeMirrorTranslator CreateDebugTranslator(
        List<RenderTreeFrame> result, 
        string circuitId,
        Type componentType,
        int componentId
    )
    {
        return new RenderTreeDebugMirrorTranslator(
            result,
            circuitId,
            componentType,
            componentId
        );
    }
}

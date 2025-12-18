 using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeTranslatorFactory
    : IRenderTreeTranslatorFactory
{
    public IRenderTreeTranslator CreateMirrorTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    )
    {
        return new RenderTreeMirrorTranslator(
            result,
            circuitId
        );
    }

    public IRenderTreeTranslator CreateDebugTranslator(
        List<RenderTreeFrame> result, 
        string circuitId,
        Type componentType,
        int componentId
    )
    {
        return new RenderTreeDebugTranslator(
            result,
            circuitId,
            componentType,
            componentId
        );
    }
}

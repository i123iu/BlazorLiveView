using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal interface IRenderTreeTranslatorFactory
{
    IRenderTreeTranslator CreateMirrorTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    );

    IRenderTreeTranslator CreateDebugTranslator(
        List<RenderTreeFrame> result,
        string circuitId,
        Type componentType,
        int componentId
    );
}

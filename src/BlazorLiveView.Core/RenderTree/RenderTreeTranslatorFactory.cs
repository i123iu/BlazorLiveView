using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeTranslatorFactory(
    ILoggerFactory loggerFactory
) : IRenderTreeTranslatorFactory
{
    public IRenderTreeTranslator CreateMirrorTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    )
    {
        return new RenderTreeMirrorTranslator(
            loggerFactory.CreateLogger<RenderTreeMirrorTranslator>(),
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
            loggerFactory.CreateLogger<RenderTreeDebugTranslator>(),
            result,
            circuitId,
            componentType,
            componentId
        );
    }
}

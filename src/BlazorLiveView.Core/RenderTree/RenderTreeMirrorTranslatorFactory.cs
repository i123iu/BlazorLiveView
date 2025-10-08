using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeMirrorTranslatorFactory(
    IServiceProvider _services
) : IRenderTreeMirrorTranslatorFactory
{
    public IRenderTreeMirrorTranslator CreateTranslator(
        List<RenderTreeFrame> result,
        string circuitId
    )
    {
        return new RenderTreeMirrorTranslator(
            result,
            circuitId,
            _services.GetRequiredService<ILogger<RenderTreeMirrorTranslator>>()
        );
    }
}

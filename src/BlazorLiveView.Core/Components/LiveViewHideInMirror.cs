using BlazorLiveView.Core.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// The contents of this component will be hidden (removed) when rendering
/// inside a mirror circuit. Also see <seealso cref="LiveViewHideInMirrorAttribute"/>.
/// </summary>
[LiveViewHideInMirror]
public class LiveViewHideInMirror(
    ILogger<LiveViewHideInMirror> logger
) : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent == null)
        {
            logger.LogWarning("{ComponentName} has no ChildContent (null)",
                nameof(LiveViewHideInMirror)
            );
            return;
        }

        ChildContent(builder);
    }
}

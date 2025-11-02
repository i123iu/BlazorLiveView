using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RendererWrapper
    : WrapperBase<Renderer>
{
    private static readonly Type InnerType = Types.Renderer;
    private static readonly MethodInfo _GetOptionalComponentState;

    static RendererWrapper()
    {
        _GetOptionalComponentState = InnerType.GetRequiredMethod(
            "GetOptionalComponentState", isPublic: false
        );
    }

    public RendererWrapper(Renderer inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public ComponentState? GetOptionalComponentState(int componentId)
        => (ComponentState?)_GetOptionalComponentState.Invoke(Inner, [componentId]);
}

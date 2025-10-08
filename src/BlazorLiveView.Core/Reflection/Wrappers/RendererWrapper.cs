using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RendererWrapper
    : WrapperBase<Renderer>
{
    private static readonly Type WrappedType = Types.Renderer;
    private static readonly MethodInfo _GetOptionalComponentState;

    static RendererWrapper()
    {
        _GetOptionalComponentState = WrappedType.GetRequiredMethod(
            "GetOptionalComponentState", isPublic: false
        );
    }

    public RendererWrapper(Renderer wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public ComponentState? GetOptionalComponentState(int componentId)
        => (ComponentState?)_GetOptionalComponentState.Invoke(Inner, [componentId]);
}

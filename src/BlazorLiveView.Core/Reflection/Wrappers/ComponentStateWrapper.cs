using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ComponentStateWrapper(ComponentState inner)
    : WrapperBase<ComponentState>(inner)
{
    private static readonly Type InnerType = Types.ComponentState;
    private static readonly PropertyInfo _CurrentRenderTree;
    private static readonly PropertyInfo _Renderer;

    static ComponentStateWrapper()
    {
        _CurrentRenderTree = InnerType.GetRequiredProperty(
            "CurrentRenderTree", isPublic: false
        );
        _Renderer = InnerType.GetRequiredProperty(
            "Renderer", isPublic: false
        );
    }

    public RenderTreeBuilder CurrentRenderTree
        => (RenderTreeBuilder)_CurrentRenderTree.GetValue(Inner)!;

    public Renderer Renderer
        => (Renderer)_Renderer.GetValue(Inner)!;
}

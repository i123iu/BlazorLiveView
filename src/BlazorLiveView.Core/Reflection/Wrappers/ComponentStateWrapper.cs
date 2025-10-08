using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ComponentStateWrapper
    : WrapperBase<ComponentState>
{
    private static readonly Type WrappedType = Types.ComponentState;
    private static readonly PropertyInfo _CurrentRenderTree;
    private static readonly PropertyInfo _Renderer;

    static ComponentStateWrapper()
    {
        _CurrentRenderTree = WrappedType.GetRequiredProperty(
            "CurrentRenderTree", isPublic: false
        );
        _Renderer = WrappedType.GetRequiredProperty(
            "Renderer", isPublic: false
        );
    }

    public ComponentStateWrapper(ComponentState wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public RenderTreeBuilder CurrentRenderTree
        => (RenderTreeBuilder)_CurrentRenderTree.GetValue(Inner)!;

    public Renderer Renderer
        => (Renderer)_Renderer.GetValue(Inner)!;
}

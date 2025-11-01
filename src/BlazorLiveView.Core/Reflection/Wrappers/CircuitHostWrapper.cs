using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class CircuitHostWrapper : WrapperBase
{
    private static readonly Type InnerType = Types.CircuitHost;
    private static readonly PropertyInfo _renderer;
    private static readonly PropertyInfo _Circuit;
    private static readonly FieldInfo _navigationManager;

    static CircuitHostWrapper()
    {
        _renderer = InnerType.GetRequiredProperty(
            "Renderer", isPublic: true
        );
        _Circuit = InnerType.GetRequiredProperty(
            "Circuit", isPublic: true
        );
        _navigationManager = InnerType.GetRequiredField(
            "_navigationManager", isPublic: false
        );
    }

    public CircuitHostWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public RemoteRendererWrapper Renderer
        => new((Renderer)_renderer.GetValue(Inner)!);

    public CircuitWrapper Circuit
        => new((Circuit)_Circuit.GetValue(Inner)!);

    public RemoteNavigationManagerWrapper NavigationManager
        => new((NavigationManager)_navigationManager.GetValue(Inner)!);
}
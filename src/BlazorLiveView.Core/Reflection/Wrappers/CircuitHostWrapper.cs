﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class CircuitHostWrapper : WrapperBase
{
    private static readonly Type WrappedType = Types.CircuitHost;
    private static readonly PropertyInfo _renderer;
    private static readonly PropertyInfo _Circuit;
    private static readonly FieldInfo _navigationManager;

    static CircuitHostWrapper()
    {
        _renderer = WrappedType.GetRequiredProperty(
            "Renderer", isPublic: true
        );
        _Circuit = WrappedType.GetRequiredProperty(
            "Circuit", isPublic: true
        );
        _navigationManager = WrappedType.GetRequiredField(
            "_navigationManager", isPublic: false
        );
    }

    public CircuitHostWrapper(object wrapped) : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public RemoteRendererWrapper Renderer
        => new((Renderer)_renderer.GetValue(Inner)!);

    public CircuitWrapper Circuit
        => new((Circuit)_Circuit.GetValue(Inner)!);

    public RemoteNavigationManagerWrapper NavigationManager
        => new((NavigationManager)_navigationManager.GetValue(Inner)!);
}
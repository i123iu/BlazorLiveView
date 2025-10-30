using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.RenderTree;

/// <summary>
/// Constructors of <see cref="RenderTreeFrame"/> are internal for Blazor. 
/// This class uses reflection to access them. 
/// </summary>
internal static class RenderTreeFrameBuilder
{
    private static readonly ConstructorInfo _elementConstructor;
    private static readonly ConstructorInfo _componentConstructor;
    private static readonly ConstructorInfo _regionConstructor;
    private static readonly ConstructorInfo _textMarkupConstructor;
    private static readonly ConstructorInfo _attributeConstructor;
    private static readonly ConstructorInfo _elementReferenceCaptureConstructor;
    private static readonly ConstructorInfo _componentReferenceCaptureConstructor;

    static RenderTreeFrameBuilder()
    {
        var constructors = typeof(RenderTreeFrame).GetConstructors(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
        );

        _elementConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 4
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(int)
                && parameters[2].ParameterType == typeof(string)
                && parameters[3].ParameterType == typeof(object);
        });

        _componentConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 5
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(int)
                && parameters[2].ParameterType == typeof(Type)
                && parameters[3].ParameterType == typeof(ComponentState)
                && parameters[4].ParameterType == typeof(object);
        });

        _regionConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(int);
        });

        _textMarkupConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(bool)
                && parameters[2].ParameterType == typeof(string);
        });

        _attributeConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 5
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(string)
                && parameters[2].ParameterType == typeof(object)
                && parameters[3].ParameterType == typeof(ulong)
                && parameters[4].ParameterType == typeof(string);
        });

        _elementReferenceCaptureConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(Action<ElementReference>)
                && parameters[2].ParameterType == typeof(string);
        });

        _componentReferenceCaptureConstructor = constructors.Single(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3
                && parameters[0].ParameterType == typeof(int)
                && parameters[1].ParameterType == typeof(Action<object>)
                && parameters[2].ParameterType == typeof(int);
        });
    }

    public static RenderTreeFrame None()
        => new();

    public static RenderTreeFrame Element(
        int sequence,
        int elementSubtreeLength,
        string elementName,
        object? elementKey = null
    ) => (RenderTreeFrame)_elementConstructor.Invoke([
        sequence, elementSubtreeLength, elementName, elementKey
    ]);

    public static RenderTreeFrame Component(
        int sequence,
        int componentSubtreeLength,
        Type componentType,
        ComponentState? componentState = null,
        object? componentKey = null
    ) => (RenderTreeFrame)_componentConstructor.Invoke([
        sequence, componentSubtreeLength, componentType, componentState, componentKey
    ]);

    public static RenderTreeFrame Region(
        int sequence,
        int regionSubtreeLength
    ) => (RenderTreeFrame)_regionConstructor.Invoke([
        sequence, regionSubtreeLength
    ]);

    public static RenderTreeFrame Text(
        int sequence,
        string textContent
    ) => (RenderTreeFrame)_textMarkupConstructor.Invoke([
        sequence, false, textContent
    ]);

    public static RenderTreeFrame Markup(
        int sequence,
        string markupContent
    ) => (RenderTreeFrame)_textMarkupConstructor.Invoke([
        sequence, true, markupContent
    ]);

    public static RenderTreeFrame Attribute(
        int sequence,
        string attributeName,
        object? attributeValue = null,
        ulong attributeEventHandlerId = 0,
        string? attributeEventUpdatesAttributeName = null
    ) => (RenderTreeFrame)_attributeConstructor.Invoke([
        sequence, attributeName, attributeValue, attributeEventHandlerId, attributeEventUpdatesAttributeName
    ]);

    public static RenderTreeFrame ElementReferenceCapture(
        int sequence,
        Action<ElementReference> elementReferenceCaptureAction,
        string elementReferenceCaptureId
    ) => (RenderTreeFrame)_elementReferenceCaptureConstructor.Invoke([
        sequence, elementReferenceCaptureAction, elementReferenceCaptureId
    ]);

    public static RenderTreeFrame ComponentReferenceCapture(
        int sequence,
        Action<object> componentReferenceCaptureAction,
        int parentFrameIndex
    ) => (RenderTreeFrame)_componentReferenceCaptureConstructor.Invoke([
        sequence, componentReferenceCaptureAction, parentFrameIndex
    ]);
}

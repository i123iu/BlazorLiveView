using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.RenderTree;

/// <summary>
/// Constructors of <see cref="RenderTreeFrame"/> is internal for Blazor. 
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
    private static readonly ConstructorInfo _defaultConstructor;

    static RenderTreeFrameBuilder()
    {
        var constructors = typeof(RenderTreeFrame).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        // Default constructor for None frame type
        _defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0)!;

        // Element constructor: (int sequence, int elementSubtreeLength, string elementName, object elementKey)
        _elementConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 4 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(int) &&
                   parameters[2].ParameterType == typeof(string) &&
                   parameters[3].ParameterType == typeof(object);
        });

        // Component constructor: (int sequence, int componentSubtreeLength, Type componentType, ComponentState componentState, object componentKey)
        _componentConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 5 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(int) &&
                   parameters[2].ParameterType == typeof(Type) &&
                   parameters[3].ParameterType == typeof(ComponentState) &&
                   parameters[4].ParameterType == typeof(object);
        });

        // Region constructor: (int sequence, int regionSubtreeLength)
        _regionConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(int);
        });

        // Text/Markup constructor: (int sequence, bool isMarkup, string textOrMarkup)
        _textMarkupConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(bool) &&
                   parameters[2].ParameterType == typeof(string);
        });

        // Attribute constructor: (int sequence, string attributeName, object attributeValue, ulong attributeEventHandlerId, string attributeEventUpdatesAttributeName)
        _attributeConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 5 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(string) &&
                   parameters[2].ParameterType == typeof(object) &&
                   parameters[3].ParameterType == typeof(ulong) &&
                   parameters[4].ParameterType == typeof(string);
        });

        // ElementReferenceCapture constructor: (int sequence, Action<ElementReference> elementReferenceCaptureAction, string elementReferenceCaptureId)
        _elementReferenceCaptureConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(Action<ElementReference>) &&
                   parameters[2].ParameterType == typeof(string);
        });

        // ComponentReferenceCapture constructor: (int sequence, Action<object> componentReferenceCaptureAction, int parentFrameIndex)
        _componentReferenceCaptureConstructor = constructors.First(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType == typeof(int) &&
                   parameters[1].ParameterType == typeof(Action<object>) &&
                   parameters[2].ParameterType == typeof(int);
        });
    }

    /// <summary>
    /// Creates a None frame (uninitialized frame)
    /// </summary>
    public static RenderTreeFrame None(int sequence = 0)
    {
        var frame = _defaultConstructor != null
            ? (RenderTreeFrame)_defaultConstructor.Invoke([])
            : new RenderTreeFrame();

        // Use reflection to set the sequence and frame type since there's no public constructor
        var sequenceField = typeof(RenderTreeFrame).GetField("SequenceField", BindingFlags.NonPublic | BindingFlags.Instance);
        var frameTypeField = typeof(RenderTreeFrame).GetField("FrameTypeField", BindingFlags.NonPublic | BindingFlags.Instance);

        sequenceField!.SetValue(frame, sequence);
        frameTypeField!.SetValue(frame, RenderTreeFrameType.None);

        return frame;
    }

    /// <summary>
    /// Creates an Element frame
    /// </summary>
    public static RenderTreeFrame Element(int sequence, int elementSubtreeLength, string elementName, object? elementKey = null)
    {
        return (RenderTreeFrame)_elementConstructor.Invoke([sequence, elementSubtreeLength, elementName, elementKey]);
    }

    /// <summary>
    /// Creates a Component frame
    /// </summary>
    public static RenderTreeFrame Component(int sequence, int componentSubtreeLength, Type componentType, ComponentState? componentState = null, object? componentKey = null)
    {
        return (RenderTreeFrame)_componentConstructor.Invoke([sequence, componentSubtreeLength, componentType, componentState, componentKey]);
    }

    /// <summary>
    /// Creates a Region frame
    /// </summary>
    public static RenderTreeFrame Region(int sequence, int regionSubtreeLength)
    {
        return (RenderTreeFrame)_regionConstructor.Invoke([sequence, regionSubtreeLength]);
    }

    /// <summary>
    /// Creates a Text frame
    /// </summary>
    public static RenderTreeFrame Text(int sequence, string textContent)
    {
        return (RenderTreeFrame)_textMarkupConstructor.Invoke([sequence, false, textContent]);
    }

    /// <summary>
    /// Creates a Markup frame
    /// </summary>
    public static RenderTreeFrame Markup(int sequence, string markupContent)
    {
        return (RenderTreeFrame)_textMarkupConstructor.Invoke([sequence, true, markupContent]);
    }

    /// <summary>
    /// Creates an Attribute frame
    /// </summary>
    public static RenderTreeFrame Attribute(int sequence, string attributeName, object? attributeValue = null, ulong attributeEventHandlerId = 0, string? attributeEventUpdatesAttributeName = null)
    {
        return (RenderTreeFrame)_attributeConstructor.Invoke([sequence, attributeName, attributeValue, attributeEventHandlerId, attributeEventUpdatesAttributeName]);
    }

    /// <summary>
    /// Creates an ElementReferenceCapture frame
    /// </summary>
    public static RenderTreeFrame ElementReferenceCapture(int sequence, Action<ElementReference> elementReferenceCaptureAction, string elementReferenceCaptureId)
    {
        return (RenderTreeFrame)_elementReferenceCaptureConstructor.Invoke([sequence, elementReferenceCaptureAction, elementReferenceCaptureId]);
    }

    /// <summary>
    /// Creates a ComponentReferenceCapture frame
    /// </summary>
    public static RenderTreeFrame ComponentReferenceCapture(int sequence, Action<object> componentReferenceCaptureAction, int parentFrameIndex)
    {
        return (RenderTreeFrame)_componentReferenceCaptureConstructor.Invoke([sequence, componentReferenceCaptureAction, parentFrameIndex]);
    }
}

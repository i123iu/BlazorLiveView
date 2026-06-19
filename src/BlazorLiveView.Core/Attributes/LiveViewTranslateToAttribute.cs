using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Attributes;

/// <summary>
/// Changes the target type of a Blazor component (<see cref="IComponent"/>)
/// when being translated to a mirror circuit. The target will have its logic
/// (code) run on the mirror cirucit.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class LiveViewTranslateToAttribute(
    Type targetComponent
) : Attribute
{
    private readonly Type? _targetComponent = targetComponent;

    public static Type? GetTranslationTarget(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
            throw new ArgumentException($"The given type {componentType.Name} is not an IComponent",
                nameof(componentType));

        return GetCustomAttribute(componentType, typeof(LiveViewTranslateToAttribute)) is LiveViewTranslateToAttribute attribute
            ? attribute._targetComponent
            : null;
    }
}

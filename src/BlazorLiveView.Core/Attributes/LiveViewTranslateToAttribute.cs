using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class LiveViewTranslateToAttribute(
    Type targetComponent
) : Attribute
{
    private Type? _targetComponent = targetComponent;

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

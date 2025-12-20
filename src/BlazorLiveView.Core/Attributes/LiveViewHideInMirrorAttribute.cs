using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Attributes;

/// <summary>
/// Marks a Blazor component (IComponent) to be hidden (removed) when rendering
/// inside a mirror circuit. Also see <seealso cref="LiveViewHideInMirror"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class LiveViewHideInMirrorAttribute : Attribute
{
    public static bool WillComponentBeHiddenInMirrorCircuits(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
            throw new ArgumentException($"The given type {componentType.Name} is not an IComponent", 
                nameof(componentType));

        return IsDefined(
            componentType,
            typeof(LiveViewHideInMirrorAttribute)
        );
    }
}

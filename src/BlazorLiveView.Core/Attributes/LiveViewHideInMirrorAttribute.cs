using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Attributes;

/// <summary>
/// Marks a Blazor component (<see cref="IComponent"/>) to be hidden (removed)
/// when being translated to a mirror circuit.
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

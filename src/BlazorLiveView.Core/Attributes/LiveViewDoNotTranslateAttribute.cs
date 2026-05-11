using BlazorLiveView.Core.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Attributes;

/// <summary>
/// Marks a Blazor component (<see cref="IComponent"/>) not to be changed when
/// being translated. So it will not be changed to <see cref="MirrorComponent"/>.
/// Blazor parameters (<c>[Parameter]</c>) on them are disallowed.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class LiveViewDoNotTranslateAttribute : Attribute
{
    public static bool WillComponentBeExcludedFromTranslation(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
            throw new ArgumentException($"The given type {componentType.Name} is not an IComponent",
                nameof(componentType));

        return IsDefined(
            componentType,
            typeof(LiveViewDoNotTranslateAttribute)
        );
    }
}

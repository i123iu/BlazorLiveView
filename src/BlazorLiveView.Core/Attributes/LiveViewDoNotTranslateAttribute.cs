using Microsoft.AspNetCore.Components;
using BlazorLiveView.Core.Components;

namespace BlazorLiveView.Core.Attributes;

/// <summary>
/// Marks a Blazor component (<see cref="IComponent"/>) to be excluded from translation. Such
/// components will not be changed to <see cref="MirrorComponent"/> when translating. Blazor
/// parameters (<c>[Parameter]</c>) on them are disallowed for now. 
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

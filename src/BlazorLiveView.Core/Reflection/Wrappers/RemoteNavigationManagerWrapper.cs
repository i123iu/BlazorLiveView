using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteNavigationManagerWrapper
    : WrapperBase<NavigationManager>
{
    public static readonly Type InnerType = Types.RemoteNavigationManager;

    public RemoteNavigationManagerWrapper(NavigationManager inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }
}

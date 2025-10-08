using Microsoft.AspNetCore.Components;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteNavigationManagerWrapper
    : WrapperBase<NavigationManager>
{
    public static readonly Type WrappedType = Types.RemoteNavigationManager;

    public RemoteNavigationManagerWrapper(NavigationManager wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }
}

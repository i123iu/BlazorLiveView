using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class WebRootComponentManagerWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.WebRootComponentManager;
    private static readonly MethodInfo _GetRequiredWebRootComponent;

    static WebRootComponentManagerWrapper()
    {
        _GetRequiredWebRootComponent = WrappedType.GetRequiredMethod(
           "GetRequiredWebRootComponent",
            isPublic: false
        );
    }

    public WebRootComponentManagerWrapper(object wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public WebRootComponentWrapper GetRequiredWebRootComponent(int ssrComponentId)
    {
        var webRootComponent = _GetRequiredWebRootComponent.Invoke(Inner, [ssrComponentId])
            ?? throw new Exception("GetRequiredWebRootComponent returned null.");
        return new WebRootComponentWrapper(webRootComponent);
    }
}

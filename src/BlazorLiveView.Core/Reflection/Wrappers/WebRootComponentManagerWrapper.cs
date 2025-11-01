using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class WebRootComponentManagerWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.WebRootComponentManager;
    private static readonly MethodInfo _GetRequiredWebRootComponent;

    static WebRootComponentManagerWrapper()
    {
        _GetRequiredWebRootComponent = InnerType.GetRequiredMethod(
           "GetRequiredWebRootComponent",
            isPublic: false
        );
    }

    public WebRootComponentManagerWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public WebRootComponentWrapper GetRequiredWebRootComponent(int ssrComponentId)
    {
        var webRootComponent = _GetRequiredWebRootComponent.Invoke(Inner, [ssrComponentId])
            ?? throw new Exception("GetRequiredWebRootComponent returned null.");
        return new WebRootComponentWrapper(webRootComponent);
    }
}

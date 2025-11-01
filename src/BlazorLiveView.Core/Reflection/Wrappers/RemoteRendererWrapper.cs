using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteRendererWrapper
    : RendererWrapper
{
    private static readonly Type InnerType = Types.RemoteRenderer;
    private static readonly MethodInfo _GetOrCreateWebRootComponentManager;

    static RemoteRendererWrapper()
    {
        _GetOrCreateWebRootComponentManager = InnerType.GetRequiredMethod(
            "GetOrCreateWebRootComponentManager", isPublic: true
        );
    }

    public RemoteRendererWrapper(Renderer inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public WebRootComponentManagerWrapper GetOrCreateWebRootComponentManager()
    {
        var manager = _GetOrCreateWebRootComponentManager.Invoke(Inner, [])
            ?? throw new Exception("GetOrCreateWebRootComponentManager returned null.");
        return new WebRootComponentManagerWrapper(manager);
    }
}

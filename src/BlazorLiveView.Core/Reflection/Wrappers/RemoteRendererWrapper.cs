using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteRendererWrapper
    : RendererWrapper
{
    private static readonly Type WrappedType = Types.RemoteRenderer;
    private static readonly MethodInfo _GetOrCreateWebRootComponentManager;

    static RemoteRendererWrapper()
    {
        _GetOrCreateWebRootComponentManager = WrappedType.GetRequiredMethod(
            "GetOrCreateWebRootComponentManager", isPublic: true
        );
    }

    public RemoteRendererWrapper(Renderer wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public WebRootComponentManagerWrapper GetOrCreateWebRootComponentManager()
    {
        var manager = _GetOrCreateWebRootComponentManager.Invoke(Inner, [])
            ?? throw new Exception("GetOrCreateWebRootComponentManager returned null.");
        return new WebRootComponentManagerWrapper(manager);
    }
}

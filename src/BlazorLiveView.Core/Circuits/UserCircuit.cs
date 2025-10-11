using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class UserCircuit(
    Circuit circuit,
    DateTime openedAt
) : CircuitBase(circuit, openedAt), IUserCircuit
{
    public event IUserCircuit.UriChangedHandler? UriChanged;
    public event IUserCircuit.ComponentRerenderedHandler? ComponentRerendered;

    public string Uri => Circuit.CircuitHost.NavigationManager.Inner.Uri;

    public int SsrComponentIdToInteractiveComponentId(int ssrComponentId)
    {
        var renderer = Circuit.CircuitHost.Renderer;
        var webRootComponentManager = renderer.GetOrCreateWebRootComponentManager();
        var webRootComponent = webRootComponentManager.GetRequiredWebRootComponent(ssrComponentId);
        return webRootComponent.InteractiveComponentId;
    }

    public void NotifyComponentRerendered(int componentId)
    {
        var componentState = GetOptionalComponentState(componentId)
            ?? throw new InvalidOperationException(
                $"Component with ID '{componentId}' not found in circuit '{Id}'."
            );
        ComponentRerendered?.Invoke(this, componentId);
    }

    public void NotifyUriChanged()
    {
        UriChanged?.Invoke(this);
    }
}

using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class UserCircuit(
    Circuit circuit
) : IUserCircuit
{
    public event IUserCircuit.UriChangedHandler? UriChanged;
    private event IUserCircuit.ComponentRerenderedHandler? ComponentRerendered;
    event IUserCircuit.ComponentRerenderedHandler? IUserCircuit.ComponentRerendered
    {
        add => ComponentRerendered += value;
        remove => ComponentRerendered -= value;
    }

    private readonly CircuitWrapper _circuit = new(circuit);

    public string Id => _circuit.Inner.Id;
    public string Uri => _circuit.CircuitHost.NavigationManager.Inner.Uri;

    internal ComponentState? GetComponentState(int componentId)
    {
        return _circuit.CircuitHost.Renderer.GetOptionalComponentState(componentId);
    }

    ComponentState? IUserCircuit.GetComponentState(int componentId)
        => GetComponentState(componentId);

    int IUserCircuit.SsrComponentIdToInteractiveComponentId(int ssrComponentId)
    {
        var renderer = _circuit.CircuitHost.Renderer;
        var webRootComponentManager = renderer.GetOrCreateWebRootComponentManager();
        var webRootComponent = webRootComponentManager.GetRequiredWebRootComponent(ssrComponentId);
        return webRootComponent.InteractiveComponentId;
    }

    void IUserCircuit.NotifyComponentRerendered(int componentId)
    {
        var componentState = GetComponentState(componentId)
            ?? throw new InvalidOperationException(
                $"Component with ID '{componentId}' not found in circuit '{Id}'."
            );
        ComponentRerendered?.Invoke(this, componentId);
    }

    void IUserCircuit.NotifyUriChanged()
    {
        UriChanged?.Invoke(this);
    }
}
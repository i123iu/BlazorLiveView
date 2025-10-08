using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class RegularCircuit(
    Circuit circuit
) : IRegularCircuit
{
    public event IRegularCircuit.UriChangedHandler? UriChanged;
    private event IRegularCircuit.ComponentRerenderedHandler? ComponentRerendered;
    event IRegularCircuit.ComponentRerenderedHandler? IRegularCircuit.ComponentRerendered
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

    ComponentState? IRegularCircuit.GetComponentState(int componentId)
        => GetComponentState(componentId);

    int IRegularCircuit.SsrComponentIdToInteractiveComponentId(int ssrComponentId)
    {
        var renderer = _circuit.CircuitHost.Renderer;
        var webRootComponentManager = renderer.GetOrCreateWebRootComponentManager();
        var webRootComponent = webRootComponentManager.GetRequiredWebRootComponent(ssrComponentId);
        return webRootComponent.InteractiveComponentId;
    }

    void IRegularCircuit.NotifyComponentRerendered(int componentId)
    {
        var componentState = GetComponentState(componentId)
            ?? throw new InvalidOperationException(
                $"Component with ID '{componentId}' not found in circuit '{Id}'."
            );
        ComponentRerendered?.Invoke(this, componentId);
    }

    void IRegularCircuit.NotifyUriChanged()
    {
        UriChanged?.Invoke(this);
    }
}
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor normally. 
/// </summary>
public interface IRegularCircuit
{
    public delegate void UriChangedHandler(IRegularCircuit circuit);
    event UriChangedHandler? UriChanged;

    internal delegate void ComponentRerenderedHandler(IRegularCircuit circuit, int componentId);
    internal event ComponentRerenderedHandler? ComponentRerendered;

    string Id { get; }
    string Uri { get; }

    internal ComponentState? GetComponentState(int componentId);
    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
}

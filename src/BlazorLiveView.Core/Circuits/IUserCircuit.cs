using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor 
/// normally (without <c>BlazorLiveView</c>). 
/// </summary>
public interface IUserCircuit
{
    public delegate void UriChangedHandler(IUserCircuit circuit);
    event UriChangedHandler? UriChanged;

    internal delegate void ComponentRerenderedHandler(IUserCircuit circuit, int componentId);
    internal event ComponentRerenderedHandler? ComponentRerendered;

    string Id { get; }
    string Uri { get; }

    internal ComponentState? GetComponentState(int componentId);
    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
}

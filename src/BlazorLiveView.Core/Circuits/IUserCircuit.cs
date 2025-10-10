using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor 
/// normally (without <c>BlazorLiveView</c>). 
/// </summary>
internal interface IUserCircuit : ICircuit
{
    public delegate void UriChangedHandler(IUserCircuit circuit);
    event UriChangedHandler? UriChanged;

    public delegate void ComponentRerenderedHandler(IUserCircuit circuit, int componentId);
    event ComponentRerenderedHandler? ComponentRerendered;

    string Uri { get; }

    ComponentState? GetComponentState(int componentId);
    int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    void NotifyUriChanged();
    void NotifyComponentRerendered(int componentId);
}

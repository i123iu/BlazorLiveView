namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor 
/// normally (without <c>BlazorLiveView</c>). 
/// </summary>
public interface IUserCircuit : ICircuit
{
    public delegate void UriChangedHandler(IUserCircuit circuit);
    event UriChangedHandler? UriChanged;

    public delegate void ComponentRerenderedHandler(IUserCircuit circuit, int componentId);
    internal event ComponentRerenderedHandler? ComponentRerendered;

    string Uri { get; }

    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
}

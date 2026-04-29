using System.Security.Claims;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor normally (without 
/// <c>BlazorLiveView</c>). 
/// </summary>
public interface IUserCircuit : ICircuit
{
    public delegate void UriChangedHandler(IUserCircuit circuit);
    event UriChangedHandler? UriChanged;

    public delegate void AuthenticationStateChangedHandler(IUserCircuit circuit);
    event AuthenticationStateChangedHandler? AuthenticationStateChanged;

    public delegate void ComponentRerenderedHandler(IUserCircuit circuit, int componentId);
    internal event ComponentRerenderedHandler? ComponentRerendered;

    public delegate void JSRuntimeInvokedHandler(IUserCircuit circuit, string identifier, CancellationToken cancellationToken, object?[]? args);
    internal event JSRuntimeInvokedHandler? JSRuntimeInvoked;

    public delegate void WindowResizedHandler(IUserCircuit circuit);
    event WindowResizedHandler? WindowResized;

    public delegate void WindowScrolledHandler(IUserCircuit circuit);
    event WindowScrolledHandler? WindowScrolled;

    string Uri { get; }
    ClaimsPrincipal User { get; }
    (int width, int height)? WindowSize { get; }
    (int scrollX, int scrollY)? ScrollPosition { get; }

    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
    internal void NotifyJSRuntimeInvoked(string identifier, CancellationToken cancellationToken, object?[]? args);
    internal void NotifyWindowResized(int width, int height);
    internal void NotifyWindowScrolled(int scrollX, int scrollY);
}

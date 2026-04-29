using BlazorLiveView.Core.Components.Tools;
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

    public delegate void UserCursorChangedHandler(IUserCircuit circuit);
    event WindowScrolledHandler? UserCursorChanged;

    public delegate void MirrorCursorChangedHandler(IUserCircuit circuit, string identifier);
    event MirrorCursorChangedHandler? MirrorCursorChanged;

    string Uri { get; }
    ClaimsPrincipal User { get; }
    Position? WindowSize { get; }
    Position? ScrollPosition { get; }
    Position? UserCursorPosition { get; }
    Dictionary<string, Position> MirrorCursorPositions { get; }
    
    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
    internal void NotifyJSRuntimeInvoked(string identifier, CancellationToken cancellationToken, object?[]? args);
    internal void NotifyWindowResized(Position size);
    internal void NotifyWindowScrolled(Position scroll);
    internal void NotifyUserCursorChanged(Position? position);
    internal void NotifyMirrorCursorChanged(string identifier, Position? position);
}

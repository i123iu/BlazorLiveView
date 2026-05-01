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

    public delegate void MirrorCircuitAddedHandler(IUserCircuit circuit, IMirrorCircuit mirrorCircuit);
    internal event MirrorCircuitAddedHandler? MirrorCircuitAdded;

    public delegate void MirrorCircuitRemovedHandler(IUserCircuit circuit, IMirrorCircuit mirrorCircuit);
    internal event MirrorCircuitRemovedHandler? MirrorCircuitRemoved;

    public delegate void AnyMirrorCircuitCursorPositionChangedHandler(IUserCircuit circuit, IMirrorCircuit mirrorCircuit);
    internal event AnyMirrorCircuitCursorPositionChangedHandler? AnyMirrorCircuitCursorPositionChanged;

    /// <summary>
    /// Mirror circuits that are currently viewing this user circuit.
    /// </summary>
    HashSet<IMirrorCircuit> MirrorCircuits { get; }

    /// <summary>
    /// Special ID (additionally to the circuit ID), that uniquely identifies
    /// a web session (one tab in a web browser; see
    /// https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage). 
    /// </summary>
    Guid? SessionId { get; }

    string Uri { get; }
    ClaimsPrincipal User { get; }
    Position? WindowSize { get; }
    Position? ScrollPosition { get; }
    Position? UserCursorPosition { get; }

    /// <summary>
    /// Finishes when a mirror circuit that wants to mirror this circuit
    /// finishes loading. Finished instantly when no mirror circuit is
    /// loading.
    /// </summary>
    internal Task WaitOnMirrorCircuitLoad();
    internal void AssignSessionId(Guid sessionId);

    internal int SsrComponentIdToInteractiveComponentId(int ssrComponentId);

    internal void NotifyMirrorCircuitAdded(IMirrorCircuit mirrorCircuit);
    internal void NotifyMirrorCircuitRemoved(IMirrorCircuit mirrorCircuit);
    internal void NotifyUriChanged();
    internal void NotifyComponentRerendered(int componentId);
    internal void NotifyJSRuntimeInvoked(string identifier, CancellationToken cancellationToken, object?[]? args);
    internal void NotifyWindowResized(Position size);
    internal void NotifyWindowScrolled(Position scroll);
    internal void NotifyUserCursorChanged(Position? position);
}

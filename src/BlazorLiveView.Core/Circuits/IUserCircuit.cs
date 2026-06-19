using BlazorLiveView.Core.Components.Tools;
using BlazorLiveView.Core.JSInterop.DotnetToJs;
using System.Security.Claims;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user that is using Blazor normally (without 
/// <c>BlazorLiveView</c>). 
/// </summary>
public interface IUserCircuit : ICircuit
{
    public delegate void UserCircuitActionHandler(
        IUserCircuit circuit
    );
    public delegate void ComponentRerenderedHandler(
        IUserCircuit circuit, int componentId
    );
    public delegate void JSRuntimeInvokedHandler(
        IUserCircuit circuit, DotnetToJsInvocation invocation
    );
    public delegate void MirrorCircuitAddedHandler(
        IUserCircuit circuit, IMirrorCircuit mirrorCircuit
    );
    public delegate void MirrorCircuitRemovedHandler(
        IUserCircuit circuit, IMirrorCircuit mirrorCircuit
    );
    public delegate void AnyMirrorCircuitCursorPositionChangedHandler(
        IUserCircuit circuit, IMirrorCircuit mirrorCircuit
    );

    public event UserCircuitActionHandler? UriChanged;
    public event UserCircuitActionHandler? AuthenticationStateChanged;
    internal event ComponentRerenderedHandler? ComponentRerendered;
    public event UserCircuitActionHandler? SessionIdAssigned;
    internal event JSRuntimeInvokedHandler? JSRuntimeInvoked;
    public event UserCircuitActionHandler? WindowResized;
    internal event UserCircuitActionHandler? WindowScrolled;
    internal event UserCircuitActionHandler? UserCursorChanged;
    internal event MirrorCircuitAddedHandler? MirrorCircuitAdded;
    internal event MirrorCircuitRemovedHandler? MirrorCircuitRemoved;
    internal event AnyMirrorCircuitCursorPositionChangedHandler?
        AnyMirrorCircuitCursorPositionChanged;
    internal event UserCircuitActionHandler? UserPermissionChanged;
    internal event UserCircuitActionHandler? ShowUserPermissionRequest;

    public enum MirrorPermissionType
    {
        Allow,
        Deny,
    }

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
    Position<int>? WindowSize { get; }
    Position<float>? ScrollPosition { get; }
    Position<float>? UserCursorPosition { get; }

    MirrorPermissionType? MirrorPermission { get; }

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
    internal void NotifyJSRuntimeInvoked(DotnetToJsInvocation invocation);
    internal void NotifyWindowResized(Position<int> size);
    internal void NotifyWindowScrolled(Position<float> scroll);
    internal void NotifyUserCursorChanged(Position<float>? position);

    void AskUserForPermission();
    internal void NotifyUserPermissionGiven(MirrorPermissionType permission);
}

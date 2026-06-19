using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Components.Tools;
using BlazorLiveView.Core.JSInterop.DotnetToJs;
using BlazorLiveView.Core.Options;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using static BlazorLiveView.Core.Circuits.IUserCircuit;

namespace BlazorLiveView.Core.Circuits;

internal sealed class UserCircuit : CircuitBase, IUserCircuit
{
    public event UserCircuitActionHandler? UriChanged;
    public event ComponentRerenderedHandler? ComponentRerendered;
    public event UserCircuitActionHandler? AuthenticationStateChanged;
    public event UserCircuitActionHandler? SessionIdAssigned;
    public event JSRuntimeInvokedHandler? JSRuntimeInvoked;
    public event UserCircuitActionHandler? WindowResized;
    public event UserCircuitActionHandler? WindowScrolled;
    public event UserCircuitActionHandler? UserCursorChanged;
    public event MirrorCircuitAddedHandler? MirrorCircuitAdded;
    public event MirrorCircuitRemovedHandler? MirrorCircuitRemoved;
    public event AnyMirrorCircuitCursorPositionChangedHandler?
        AnyMirrorCircuitCursorPositionChanged;
    public event UserCircuitActionHandler? UserPermissionChanged;
    public event UserCircuitActionHandler? ShowUserPermissionRequest;

    public HashSet<IMirrorCircuit> MirrorCircuits { get; } = new();
    public string Uri => Circuit.CircuitHost.NavigationManager.Inner.Uri;
    public Guid? SessionId { get; private set; } = null;
    public ClaimsPrincipal User => _authenticationStateProvider
        // Calling .Result should be fine, since the task is created in Blazor
        // Server using `Task.FromResult` (in `CircuitHost.cs`).
        .GetAuthenticationStateAsync().Result.User;
    public Position<int>? WindowSize { get; private set; }
    public Position<float>? ScrollPosition { get; private set; }
    public Position<float>? UserCursorPosition { get; private set; }

    public MirrorPermissionType? MirrorPermission
    {
        get => _liveViewOptions.Value.RequireUserAgreement
            ? _mirrorPermission
            : MirrorPermissionType.Allow;
        set
        {
            if (_liveViewOptions.Value.RequireUserAgreement)
                _mirrorPermission = value;
        }
    }

    private MirrorPermissionType? _mirrorPermission = null;
    private readonly IPausedCircuitsTracker _pausedCircuitsTracker;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IOptions<LiveViewOptions> _liveViewOptions;

    public UserCircuit(
        Circuit circuit,
        DateTime openedAt,
        IPausedCircuitsTracker pausedCircuitsTracker,
        IOptions<LiveViewOptions> liveViewOptions,
        ILogger<UserCircuit> logger
    ) : base(circuit, openedAt, logger)
    {
        _authenticationStateProvider = Circuit.CircuitHost.Services
            .GetService<AuthenticationStateProvider>()
                ?? throw new InvalidOperationException(
                    "The AuthenticationStateProvider service is not available."
                );
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        _pausedCircuitsTracker = pausedCircuitsTracker;
        _liveViewOptions = liveViewOptions;
    }

    public override void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose();
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> _)
    {
        AuthenticationStateChanged?.Invoke(this);
    }

    public int SsrComponentIdToInteractiveComponentId(int ssrComponentId)
    {
        var renderer = Circuit.CircuitHost.Renderer;
        var webRootComponentManager = renderer.GetOrCreateWebRootComponentManager();
        var webRootComponent = webRootComponentManager.GetRequiredWebRootComponent(ssrComponentId);
        return webRootComponent.InteractiveComponentId;
    }

    public void AssignSessionId(Guid sessionId)
    {
        if (SessionId != null)
        {
            throw new InvalidOperationException("Session ID is already assigned.");
        }
        SessionId = sessionId;
        SessionIdAssigned?.Invoke(this);
    }

    public void NotifyComponentRerendered(int componentId)
    {
        var componentState = GetOptionalComponentState(componentId)
            ?? throw new InvalidOperationException(
                $"Component with ID '{componentId}' not found in circuit '{Id}'."
            );
        ComponentRerendered?.Invoke(this, componentId);
    }

    public void NotifyUriChanged()
    {
        UriChanged?.Invoke(this);
    }

    public void NotifyJSRuntimeInvoked(DotnetToJsInvocation invocation)
    {
        JSRuntimeInvoked?.Invoke(this, invocation);
    }

    public void NotifyWindowResized(Position<int> size)
    {
        WindowSize = size;
        WindowResized?.Invoke(this);
    }

    public void NotifyWindowScrolled(Position<float> scroll)
    {
        ScrollPosition = scroll;
        WindowScrolled?.Invoke(this);
    }

    public void NotifyUserCursorChanged(Position<float>? position)
    {
        UserCursorPosition = position;
        UserCursorChanged?.Invoke(this);
    }

    public void NotifyMirrorCircuitAdded(IMirrorCircuit mirrorCircuit)
    {
        if (MirrorCircuits.Contains(mirrorCircuit)) return;
        MirrorCircuits.Add(mirrorCircuit);
        MirrorCircuitAdded?.Invoke(this, mirrorCircuit);
        mirrorCircuit.CursorPositionChanged += OnAnyMirrorCircuitCursorPositionChanged;
    }

    public void NotifyMirrorCircuitRemoved(IMirrorCircuit mirrorCircuit)
    {
        if (!MirrorCircuits.Contains(mirrorCircuit)) return;
        MirrorCircuits.Remove(mirrorCircuit);
        MirrorCircuitRemoved?.Invoke(this, mirrorCircuit);
        mirrorCircuit.CursorPositionChanged -= OnAnyMirrorCircuitCursorPositionChanged;
    }

    private void OnAnyMirrorCircuitCursorPositionChanged(IMirrorCircuit mirrorCircuit)
    {
        AnyMirrorCircuitCursorPositionChanged?.Invoke(this, mirrorCircuit);
    }

    public void AskUserForPermission()
    {
        if (!_liveViewOptions.Value.RequireUserAgreement) return;
        if (MirrorPermission is not null &&
            MirrorPermission == MirrorPermissionType.Deny) return;
        ShowUserPermissionRequest?.Invoke(this);
    }

    public void NotifyUserPermissionGiven(MirrorPermissionType permission)
    {
        if (MirrorPermission == permission) return;
        MirrorPermission = permission;
        UserPermissionChanged?.Invoke(this);
    }

    Task IUserCircuit.WaitOnMirrorCircuitLoad()
    {
        return _pausedCircuitsTracker.WaitOnResume(this);
    }

    public override int GetHashCode()
        => Id.GetHashCode();
}

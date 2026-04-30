using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Components.Tools;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BlazorLiveView.Core.Circuits;

internal sealed class UserCircuit : CircuitBase, IUserCircuit
{
    public event IUserCircuit.UriChangedHandler? UriChanged;
    public event IUserCircuit.ComponentRerenderedHandler? ComponentRerendered;
    public event IUserCircuit.AuthenticationStateChangedHandler? AuthenticationStateChanged;
    public event IUserCircuit.JSRuntimeInvokedHandler? JSRuntimeInvoked;
    public event IUserCircuit.WindowResizedHandler? WindowResized;
    public event IUserCircuit.WindowScrolledHandler? WindowScrolled;
    public event IUserCircuit.WindowScrolledHandler? UserCursorChanged;
    public event IUserCircuit.MirrorCircuitAddedHandler? MirrorCircuitAdded;
    public event IUserCircuit.MirrorCircuitRemovedHandler? MirrorCircuitRemoved;
    public event IUserCircuit.AnyMirrorCircuitCursorPositionChangedHandler? AnyMirrorCircuitCursorPositionChanged;

    public HashSet<IMirrorCircuit> MirrorCircuits { get; } = new();
    public string Uri => Circuit.CircuitHost.NavigationManager.Inner.Uri;
    public ClaimsPrincipal User => _authenticationStateProvider
        // Calling .Result should be fine, since the task is created in Blazor
        // Server using `Task.FromResult` (in `CircuitHost.cs`).
        .GetAuthenticationStateAsync().Result.User;
    public Position? WindowSize { get; private set; }
    public Position? ScrollPosition { get; private set; }
    public Position? UserCursorPosition { get; private set; }

    private readonly IPausedCircuitsTracker _pausedCircuitsTracker;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserCircuit(
        Circuit circuit,
        DateTime openedAt,
        IPausedCircuitsTracker pausedCircuitsTracker,
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

    public void NotifyJSRuntimeInvoked(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        JSRuntimeInvoked?.Invoke(this, identifier, cancellationToken, args);
    }

    public void NotifyWindowResized(Position size)
    {
        WindowSize = size;
        WindowResized?.Invoke(this);
    }

    public void NotifyWindowScrolled(Position scroll)
    {
        ScrollPosition = scroll;
        WindowScrolled?.Invoke(this);
    }

    public void NotifyUserCursorChanged(Position? position)
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

    Task IUserCircuit.WaitOnMirrorCircuitLoad()
    {
        return _pausedCircuitsTracker.WaitOnResume(this);
    }

    public override int GetHashCode()
        => Id.GetHashCode();
}

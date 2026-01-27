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

    public string Uri => Circuit.CircuitHost.NavigationManager.Inner.Uri;
    public ClaimsPrincipal User => _authenticationStateProvider
        // Calling .Result should be fine, since the task is created in Blazor
        // Server using `Task.FromResult` (in `CircuitHost.cs`).
        .GetAuthenticationStateAsync().Result.User;

    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserCircuit(
        Circuit circuit,
        DateTime openedAt,
        ILogger<UserCircuit> logger
    ) : base(circuit, openedAt, logger)
    {
        _authenticationStateProvider = Circuit.CircuitHost.Services
            .GetService<AuthenticationStateProvider>()
                ?? throw new InvalidOperationException(
                    "The AuthenticationStateProvider service is not available."
                );
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
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
}

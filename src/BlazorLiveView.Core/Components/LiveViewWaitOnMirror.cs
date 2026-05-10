using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Circuits.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// Wrapper for app content that will first wait on any mirror circuits that
/// want to view this circuit to finish loading. This allows the mirror
/// circuits to capture all JS interop calls that would normally happen
/// before the mirror circuit could finish loading.
/// </summary>
public class LiveViewWaitOnMirror(
    ICurrentCircuit currentCircuit,
    ILogger<LiveViewWaitOnMirror> logger
) : ComponentBase
{
    private const int TIMEOUT_MS = 4_000;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private readonly ICurrentCircuit _currentCircuit = currentCircuit;
    private readonly ILogger<LiveViewWaitOnMirror> _logger = logger;

    private bool _showChildContent = false;
    private bool _sessionIdRetrieved = false;
    private IUserCircuit _userCircuit = null!;

    protected override void OnInitialized()
    {
        if (!RendererInfo.IsInteractive)
        {
            _showChildContent = true;
            return;
        }

        var circuit = _currentCircuit.AsLiveViewCircuit();
        if (circuit is null)
        {
            _showChildContent = true;
            _logger.LogWarning("Could not find circuit.");
            return;
        }
        else if (circuit is IMirrorCircuit)
        {
            _showChildContent = true;
            _logger.LogWarning("Rendered in a mirror circuit.");
            return;
        }
        else if (circuit is IUserCircuit userCircuit)
        {
            _userCircuit = userCircuit;
            _userCircuit.UserPermissionChanged += OnUserCircuitPermissionChanged;
            if (_userCircuit.SessionId.HasValue)
            {
                _sessionIdRetrieved = true;
                WaitOnMirrorCircuitLoad();
                return;
            }

            // Continue rendering if session id retrieval timeouts
            Task.Delay(TIMEOUT_MS).ContinueWith(t =>
            {
                InvokeAsync(() =>
                {
                    if (!_sessionIdRetrieved && !_showChildContent)
                    {
                        _showChildContent = true;
                        _logger.LogWarning(
                            "Timeout while waiting for OnSessionIdRetrieved."
                        );
                        StateHasChanged();
                    }
                });
            });
        }
    }

    private void OnUserCircuitPermissionChanged(IUserCircuit userCircuit)
    {
        if (userCircuit.MirrorPermission == IUserCircuit.MirrorPermissionType.Deny)
        {
            // Stop waiting
            _showChildContent = true;
            StateHasChanged();
        }
    }

    private Task OnSessionIdRetrieved(Guid sessionId)
    {
        if (_sessionIdRetrieved) return Task.CompletedTask;
        _sessionIdRetrieved = true;
        _userCircuit.AssignSessionId(sessionId);
        WaitOnMirrorCircuitLoad();
        return Task.CompletedTask;
    }

    private void WaitOnMirrorCircuitLoad()
    {
        _userCircuit.WaitOnMirrorCircuitLoad().ContinueWith(task =>
        {
            InvokeAsync(() =>
            {
                _showChildContent = true;
                StateHasChanged();
            });
        });
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent == null)
        {
            _logger.LogWarning("{ComponentName} has no ChildContent (null)",
                nameof(LiveViewWaitOnMirror)
            );
            return;
        }

        // This child component will not be translated to a mirror component
        // (see its attribute) and will notify the mirror circuit when it
        // finishes loading. This will complete WaitOnMirrorCircuitLoad.
        builder.OpenComponent<LiveViewLoadIndicator>(0);
        builder.CloseComponent();

        if (!_sessionIdRetrieved)
        {
            builder.OpenComponent<LiveViewSessionDetector>(1);
            builder.AddAttribute(2,
                nameof(LiveViewSessionDetector.OnSessionIdRetrieved),
                EventCallback.Factory.Create<Guid>(this, OnSessionIdRetrieved)
            );
            builder.CloseComponent();
        }

        if (_showChildContent)
        {
            builder.AddContent(1, ChildContent);
        }
    }
}

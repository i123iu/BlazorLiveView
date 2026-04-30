using BlazorLiveView.Core.Attributes;
using BlazorLiveView.Core.Circuits.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Components;

/// <summary>
/// This component is used with <see cref="LiveViewWaitOnMirror"/> and notifies
/// circuits that this component has been rendered.
/// </summary>
[LiveViewDoNotTranslate]
internal class LiveViewLoadIndicator(
    ICurrentCircuit currentCircuit,
    ILogger<LiveViewWaitOnMirror> logger
) : ComponentBase
{
    private readonly ICurrentCircuit _currentCircuit = currentCircuit;
    private readonly ILogger<LiveViewWaitOnMirror> _logger = logger;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        var circuit = _currentCircuit.AsLiveViewCircuit();
        if ( circuit is null)
        {
            _logger.LogWarning("Could not find circuit.");
            return;
        }
        else
        {
            circuit.NotifyLoaded();
        }
    }
}

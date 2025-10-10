using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Core.Circuits;

internal sealed class LiveViewCircuitHandler(
    ICircuitTracker circuitTracker
) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(
        Circuit circuit,
        CancellationToken cancellationToken
    )
    {
        circuitTracker.CircuitOpened(circuit);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(
        Circuit circuit, 
        CancellationToken cancellationToken
    )
    {
        circuitTracker.CircuitUp(circuit);
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(
        Circuit circuit, 
        CancellationToken cancellationToken
    )
    {
        circuitTracker.CircuitDown(circuit);
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(
        Circuit circuit,
        CancellationToken cancellationToken
    )
    {
        circuitTracker.CircuitClosed(circuit);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

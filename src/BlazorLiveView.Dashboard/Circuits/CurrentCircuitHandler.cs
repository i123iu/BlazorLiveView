using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorLiveView.Dashboard.Circuits;

internal sealed class CurrentCircuitHandler(
    ICurrentCircuit currentCircuit
) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        currentCircuit.Current = circuit;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }
}

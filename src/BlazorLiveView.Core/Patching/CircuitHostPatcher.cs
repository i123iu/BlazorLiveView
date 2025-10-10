using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

internal sealed class CircuitHostPatcher : IPatcher
{
    private static ICircuitTracker _circuitTracker = null!;
    private static ILogger<CircuitHostPatcher> _logger = null!;

    public CircuitHostPatcher(
        ICircuitTracker circuitTracker,
        ILogger<CircuitHostPatcher> logger
    )
    {
        _circuitTracker = circuitTracker;
        _logger = logger;
    }

    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(
                Types.CircuitHost,
                "OnLocationChangedAsync"
            ),
            postfix: new HarmonyMethod(
                OnLocationChangedAsync_Postfix
            )
        );
    }

    private static void OnLocationChangedAsync_Postfix(
        object __instance
    )
    {
        CircuitHostWrapper circuitHost = new(__instance);
        var circuitId = circuitHost.Circuit.Inner.Id;
        var circuit = _circuitTracker.GetCircuit(circuitId);
        if (circuit is null)
        {
            _logger.LogError("Circuit {CircuitId} not found",
                circuitId);
            return;
        }

        if (circuit is IUserCircuit userCircuit)
        {
            userCircuit.NotifyUriChanged();
        }
    }
}

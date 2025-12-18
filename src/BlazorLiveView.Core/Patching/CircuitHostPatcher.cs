using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// This patcher is responsible for notifying user circuits about URI changes
/// (so that mirror circuits can be updated). 
/// </summary>
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

        harmony.Patch(
            AccessTools.Method(
                Types.CircuitHost,
                "BeginInvokeDotNetFromJS"
            ),
            prefix: new HarmonyMethod(
                BeginInvokeDotNetFromJS_Prefix
            )
        );
    }

    /// <summary>
    /// Called when the location (URL) of the client's window has changed. 
    /// Update its mirror circuits. 
    /// </summary>
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

    private static bool BeginInvokeDotNetFromJS_Prefix(
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
            return true;
        }

        if (circuit is IMirrorCircuit)
        {
            // Skip the original function
            // TODO?: passing e.g. button clicks to the original user circuit
            _logger.LogDebug("Skipped BeginInvokeDotNetFromJS for mirror circuit id={Id}. ", 
                circuit.Id);
            return false;
        }

        return true;
    }
}

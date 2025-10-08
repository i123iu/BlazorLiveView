using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

internal sealed class ComponentStatePatcher : IPatcher
{
    private static ICircuitTracker _circuitTracker = null!;
    private static ILogger<ComponentStatePatcher> _logger = null!;

    public ComponentStatePatcher(
        ICircuitTracker circuitTracker,
        ILogger<ComponentStatePatcher> logger
    )
    {
        _circuitTracker = circuitTracker;
        _logger = logger;
    }

    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(
                Types.ComponentState,
                "RenderIntoBatch"
            ),
            postfix: new HarmonyMethod(
                RenderIntoBatch_Postfix
            )
        );
    }

    private static void RenderIntoBatch_Postfix(ComponentState __instance)
    {
        ComponentStateWrapper wrapped = new(__instance);
        var renderer = wrapped.Renderer;
        if (!renderer.GetType().Equals(Types.RemoteRenderer))
        {
            return;
        }

        RemoteRendererWrapper remoteRendererWrapped = new(renderer);
        var circuit = _circuitTracker.GetCircuit(remoteRendererWrapped);

        if (circuit is null)
        {
            _logger.LogWarning(
                "No regular circuit found for RemoteRenderer {RemoteRenderer}.",
                remoteRendererWrapped
            );
            return;
        }

        if (circuit.IsMirror)
        {
            // Mirrors cannot be mirrored again. 
            return;
        }

        circuit.RegularCircuit.NotifyComponentRerendered(__instance.ComponentId);
    }
}

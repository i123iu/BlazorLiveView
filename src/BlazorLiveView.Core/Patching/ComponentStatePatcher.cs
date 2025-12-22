using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// This patcher is responsible for notifying user circuits about component
/// rerenders (so that mirror circuits can trigger mirror component rerenders).
/// </summary>
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

        if (wrapped.ComponentWasDisposed)
        {
            return;
        }

        var renderer = wrapped.Renderer;
        if (!renderer.GetType().Equals(Types.RemoteRenderer))
        {
            return;
        }

        RemoteRendererWrapper remoteRendererWrapped = new(renderer);
        var circuit = _circuitTracker.GetCircuit(remoteRendererWrapped);

        if (circuit is null)
        {
            // This can happen if the circuit has been closed,
            // but the component has not been disposed yet.
            return;
        }

        if (circuit is not IUserCircuit userCircuit)
        {
            // Mirrors cannot be mirrored again.
            return;
        }

        userCircuit.NotifyComponentRerendered(__instance.ComponentId);
    }
}

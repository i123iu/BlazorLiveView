using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Components;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// This patcher is responsible for creating RootMirrorComponent instead of the
/// original root component for mirror circuits. These new root mirror
/// components are then linked to their source circuit's root component.
/// </summary>
internal sealed class WebRootComponentPatcher : IPatcher
{
    private static ICircuitTracker _circuitTracker = null!;
    private static ILogger<WebRootComponentPatcher> _logger = null!;

    public WebRootComponentPatcher(
        ICircuitTracker circuitTracker,
        ILogger<WebRootComponentPatcher> logger
    )
    {
        _circuitTracker = circuitTracker;
        _logger = logger;
    }

    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(
                Types.WebRootComponent,
                "Create"
            ),
            prefix: new HarmonyMethod(
                Create_Prefix
            ),
            postfix: new HarmonyMethod(
                Create_Postfix
            )
        );
    }

    private static void Create_Prefix(
        Renderer renderer, // This is `RemoteRenderer`
        ref Type componentType,
        int ssrComponentId, // This is mirror's SSR component ID
        out State __state
    )
    {
        __state = new State { overwritten = false };

        RemoteRendererWrapper remoteRenderer = new(renderer);
        var circuit = _circuitTracker.GetCircuit(remoteRenderer);
        if (circuit is null)
        {
            _logger.LogError("Circuit for RemoteRenderer not found");
            return;
        }

        if (circuit is not IMirrorCircuit mirrorCircuit)
        {
            return;
        }

        _logger.LogInformation("Overwriting root component {OriginalRootComponentType}" +
            $" to {nameof(RootMirrorComponent)} for IMirrorCircuit with id={{CircuitId}}",
            componentType,
            mirrorCircuit.Id
        );

        componentType = typeof(RootMirrorComponent);

        __state = new State
        {
            overwritten = true,
            mirrorSsrComponentId = ssrComponentId,
            mirrorCircuit = mirrorCircuit
        };
    }

    private static void Create_Postfix(
        object __result,
        State __state
    )
    {
        if (!__state.overwritten)
        {
            return;
        }

        // This is the overwritten `RootMirrorComponent`
        WebRootComponentWrapper webRootComponent = new(__result);
        var interactiveComponentId = webRootComponent.InteractiveComponentId;

        var componentState = __state.mirrorCircuit
            .GetOptionalComponentState(interactiveComponentId)
            ?? throw new Exception($"Component with ID '{interactiveComponentId}' not found in circuit '{__state.mirrorCircuit.Id}'.");
        if (componentState.Component is not RootMirrorComponent rootMirrorComponent)
        {
            throw new Exception($"Component is not {nameof(RootMirrorComponent)}.");
        }

        rootMirrorComponent.Initialize(
            __state.mirrorCircuit.SourceCircuit,
            __state.mirrorSsrComponentId
        );
    }

    private struct State
    {
        public bool overwritten;
        public int mirrorSsrComponentId;
        public IMirrorCircuit mirrorCircuit;
    }
}

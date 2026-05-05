using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.JSInterop;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// This patcher is responsible for notifying user circuits about URI changes
/// (so that mirror circuits can be updated). 
/// </summary>
internal sealed class CircuitHostPatcher : IPatcher
{
    private static ICircuitTracker _circuitTracker = null!;
    private static LiveViewJSInteropOptions _jsInteropOptions = null!;
    private static ILogger<CircuitHostPatcher> _logger = null!;
    private static IPatchExceptionHandler _patchExceptionHandler = null!;

    public CircuitHostPatcher(
        ICircuitTracker circuitTracker,
        IOptions<LiveViewJSInteropOptions> jsInteropOptions,
        ILogger<CircuitHostPatcher> logger,
        IPatchExceptionHandler patchExceptionHandler
    )
    {
        _circuitTracker = circuitTracker;
        _jsInteropOptions = jsInteropOptions.Value;
        _logger = logger;
        _patchExceptionHandler = patchExceptionHandler;
    }

    [ExcludeFromCodeCoverage]
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
    /// </summary>
    private static void OnLocationChangedAsync_Postfix(
        object __instance
    )
    {
        try
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
                // Update the mirror circuits mirroring this user circuit
                userCircuit.NotifyUriChanged();
            }
            else if (circuit is IMirrorCircuit mirrorCircuit)
            {
                // The location of a mirror circuit should not change (it should
                // always be <see cref="LiveViewOptions.MirrorUri"/>).
                mirrorCircuit.SetBlocked(new MirrorCircuitBlockReason(
                    "Mirror circuit's location changed. "
                ));
            }
        }
        catch (Exception ex)
        {
            _patchExceptionHandler.LogPostfixException(_logger, nameof(OnLocationChangedAsync_Postfix), ex);
        }
    }

    /// <summary>
    /// Called for example on events like "onclick". Mirror circuits should not
    /// be able to interact with the app, so the original function is skipped.
    /// </summary>
    private static bool BeginInvokeDotNetFromJS_Prefix(
        object __instance,
        string methodIdentifier
    )
    {
        try
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
                var forwardingBehaviour = _jsInteropOptions.JsToDotnetForwardingRules
                    .GetForwardingBehavior(methodIdentifier);
                if (forwardingBehaviour == ForwardingBehavior.Forward)
                {
                    return true;
                }

                // Skip the original function
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Skipped BeginInvokeDotNetFromJS method={Method} for mirror circuit id={Id}. ",
                        methodIdentifier, circuit.Id
                    );
                }

                // The original function BeginInvokeDotNetFromJS calls the requested
                // method and sends "JS.EndInvokeDotNet" after finishing. Skipping
                // the function means also skipping the response. The web app will
                // probably wait for the response indefinitely, but this should not
                // be a problem as the mirror user should not interact with the app
                // anyway. 
                return false;
            }
        }
        catch (Exception ex)
        {
            _patchExceptionHandler.LogPrefixException(_logger, nameof(BeginInvokeDotNetFromJS_Prefix), ex);
        }

        return true;
    }
}

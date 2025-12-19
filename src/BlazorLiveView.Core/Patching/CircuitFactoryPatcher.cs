using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using BlazorLiveView.Core.UriHelpers;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// This patcher is responsible for mirror circuit creation. To identify mirror
/// circuits from user circuits, their request endpoint is set to the mirror
/// endpoint and here it is fixed (overwritten/changed to the source circuit's
/// endpoint).
/// </summary>
internal sealed class CircuitFactoryPatcher : IPatcher
{
    private static ICircuitTracker _circuitTracker = null!;
    private static LiveViewOptions _liveViewOptions = null!;
    private static ILiveViewMirrorUriBuilder _mirrorUriBuilder = null!;
    private static ILogger<CircuitFactoryPatcher> _logger = null!;
    private static IPatchExceptionHandler _patchExceptionHandler = null!;

    public CircuitFactoryPatcher(
        ICircuitTracker circuitTracker,
        IOptions<LiveViewOptions> options,
        ILiveViewMirrorUriBuilder mirrorUriBuilder,
        ILogger<CircuitFactoryPatcher> logger,
        IPatchExceptionHandler patchExceptionHandler
    )
    {
        _circuitTracker = circuitTracker;
        _liveViewOptions = options.Value;
        _mirrorUriBuilder = mirrorUriBuilder;
        _logger = logger;
        _patchExceptionHandler = patchExceptionHandler;
    }

    public void Patch(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(
                Types.CircuitFactory,
                "CreateCircuitHostAsync"
            ),
            prefix: new HarmonyMethod(
                CreateCircuitHostAsync_Prefix
            ),
            postfix: new HarmonyMethod(
                CreateCircuitHostAsync_Postfix
            )
        );
    }

    private struct State
    {
        public bool errored;
        public bool isMirror;
        public IUserCircuit? sourceCircuit;
        public bool debugView;
    }

    private static void CreateCircuitHostAsync_Prefix(
        string baseUri,
        ref string uri,
        out State __state
    )
    {
        __state = new State
        {
            errored = false,
            isMirror = false,
            sourceCircuit = null,
        };

        try
        {
            if (!TryGetUriPath(baseUri, uri, out var path))
            {
                // Invalid URI
                // Should be invalid for Blazor as well
                _logger.LogWarning("Invalid URI: {Uri}", uri);
                return;
            }

            path = path.TrimStart('/');
            var liveViewPath = _liveViewOptions.MirrorUri.AsSpan().TrimStart('/');
            if (!path.StartsWith(liveViewPath, StringComparison.OrdinalIgnoreCase))
            {
                // Not targeting mirror path
                return;
            }

            if (!_mirrorUriBuilder.TryParse(new Uri(uri), out var parsedUri))
            {
                // Invalid path should have triggered an error in the mirror endpoint controller
                _logger.LogError("Failed to parse mirror URI: {Uri}", uri);
                return;
            }

            var sourceCircuit = _circuitTracker.GetCircuit(parsedUri.sourceCircuitId);
            if (sourceCircuit is null)
            {
                // This should have been caught in the mirror endpoint controller
                _logger.LogWarning("No circuit found with ID: {CircuitId}",
                    parsedUri.sourceCircuitId);
                return;
            }

            if (sourceCircuit is not IUserCircuit sourceUserCircuit)
            {
                // Also should have been caught in the mirror endpoint controller
                _logger.LogWarning("Cannot mirror a mirror circuit. Circuit ID: {CircuitId}",
                    parsedUri.sourceCircuitId);
                return;
            }

            // "Redirect" the mirror to the source's URI
            uri = sourceUserCircuit.Uri;

            __state = new State
            {
                isMirror = true,
                sourceCircuit = sourceUserCircuit,
                debugView = parsedUri.debugView
            };
        }
        catch (Exception ex)
        {
            __state.errored = true;
            _patchExceptionHandler.LogPrefixException(_logger, nameof(CreateCircuitHostAsync_Prefix), ex);
        }
    }

    private static bool TryGetUriPath(
        ReadOnlySpan<char> baseUri,
        ReadOnlySpan<char> uri,
        out ReadOnlySpan<char> path
    )
    {
        baseUri = baseUri.TrimEnd('/');

        if (uri.StartsWith(baseUri, StringComparison.Ordinal))
        {
            path = uri[baseUri.Length..];
            return true;
        }

        path = default;
        return false;
    }

    private static void CreateCircuitHostAsync_Postfix(
        ref object __result,
        string uri,
        State __state
    )
    {
        if (__state.errored)
        {
            return;
        }

        if (!__state.isMirror)
        {
            return;
        }

        // The original `CreateCircuitHostAsync` method is async and returns
        // a Task<CircuitHost>. We need to intercept the `CircuitHost` before it
        // is returned and the connection is initialized. CircuitHost is an
        // internal type so we need to use reflection.

        try
        {
            ValueTaskOfCircuitHostWrapper valueTask = new(__result);
            var task = valueTask.AsTask();
            var continuationDelegate = CreateDelegate(__state);

            var newTask = task.ContinueWith(continuationDelegate);
            var newValueTask = ValueTaskOfCircuitHostWrapper.Constructor(newTask)!;
            __result = newValueTask.Inner;
        }
        catch (Exception ex)
        {
            _patchExceptionHandler.LogPostfixException(_logger, nameof(CreateCircuitHostAsync_Postfix), ex);
        }
    }

    private static Delegate CreateDelegate(State state)
    {
        ContinuationHelper helper = new(state);
        var helperMethod = typeof(ContinuationHelper)
            .GetMethod(nameof(ContinuationHelper.Continuation), BindingFlags.Instance | BindingFlags.Public)!
            .MakeGenericMethod(Types.CircuitHost);
        return Delegate.CreateDelegate(Types.FuncOfTaskOfCircuitHostAndCircuitHost, helper, helperMethod);
    }

    private sealed class ContinuationHelper(State state)
    {
        private readonly State _state = state;

        public T Continuation<T>(Task<T> task)
        {
            T taskResult;
            try
            {
                taskResult = task.Result;
            }
            catch (Exception ex)
            {
                _patchExceptionHandler.LogPostfixException(_logger, nameof(CreateCircuitHostAsync_Postfix), ex);
                throw new Exception("Could not get result of task. ", ex);
            }

            try
            {
                object? circuitHostObj = (object?)taskResult
                    ?? throw new InvalidCastException($"{nameof(circuitHostObj)} cannot be cast to object?. ");
                var circuitHost = new CircuitHostWrapper(circuitHostObj);

                _circuitTracker!.MirrorCircuitCreated(
                    circuitHost.Circuit.Inner,
                    _state.sourceCircuit ?? throw new Exception(),
                    _state.debugView
                );
            }
            catch (Exception ex)
            {
                _patchExceptionHandler.LogPostfixException(_logger, nameof(CreateCircuitHostAsync_Postfix), ex);
            }

            return taskResult;
        }
    }
}

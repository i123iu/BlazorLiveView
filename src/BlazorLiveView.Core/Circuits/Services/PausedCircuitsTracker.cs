using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Circuits.Services;

internal class PausedCircuitsTracker : IPausedCircuitsTracker, IDisposable
{
    private const long TIMEOUT_MS = 10_000;

    private readonly ICircuitTracker _circuitTracker;
    private readonly ILogger<PausedCircuitsTracker> _logger;

    private readonly Lock _lock = new();

    /// <summary>
    /// Session IDs of circuits whose next circuit will be paused.
    /// Access only with _lock.
    /// </summary>
    private readonly Dictionary<Guid, PausedCircuitReference> _usersToBePaused = new();

    /// <summary>
    /// 
    /// Access only with _lock.
    /// </summary>
    private readonly Dictionary<IUserCircuit, TaskCompletionSource> _pausedCircuits
        = new();

    public PausedCircuitsTracker(
        ICircuitTracker circuitTracker,
        ILogger<PausedCircuitsTracker> logger
    )
    {
        _circuitTracker = circuitTracker;
        _logger = logger;

        _circuitTracker.OnCircuitClosed += OnCircuitClosed;
    }

    public void Dispose()
    {
        _circuitTracker.OnCircuitClosed -= OnCircuitClosed;
    }

    private void OnCircuitClosed(ICircuit circuit)
    {
        if (circuit is IUserCircuit userCircuit)
        {
            lock (_lock)
            {
                if (!_pausedCircuits.Remove(userCircuit, out var _))
                    return;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Circuit {CircuitId} was closed while paused.",
                        circuit.Id
                    );
                }
            }
        }
        else if (circuit is IMirrorCircuit mirrorCircuit)
        {
            lock (_lock)
            {
                if (!_pausedCircuits.Remove(mirrorCircuit.SourceCircuit, out var tcs))
                    return;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Mirror circuit {CircuitId} was closed while source circuit {SourceCircuitId} was paused.",
                        mirrorCircuit.Id,
                        mirrorCircuit.SourceCircuit.Id
                    );
                }

                if (!tcs.TrySetResult())
                {
                    _logger.LogError(
                        "Could not resume tcs for circuit {CircuitId} " +
                        "after mirror circuit was closed",
                        mirrorCircuit.SourceCircuit.Id
                    );
                }
            }
        }
    }

    public PausedCircuitReference? MarkPaused(Guid circuitSessionId)
    {
        lock (_lock)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Pausing session {SessionId}",
                    circuitSessionId
                );
            }

            PausedCircuitReference reference = new(circuitSessionId);
            reference.Unpaused += PausedCircuitReference_UnpauseUser;
            if (_usersToBePaused.TryAdd(circuitSessionId, reference))
            {
                return reference;
            }
            return null;
        }
    }

    private void PausedCircuitReference_UnpauseUser(
        PausedCircuitReference reference
    )
    {
        lock (_lock)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Unpausing session {SessionId}",
                    reference.circuitSessionId
                );
            }
            reference.Unpaused -= PausedCircuitReference_UnpauseUser;
            _usersToBePaused.Remove(reference.circuitSessionId);
        }
    }

    public async Task WaitOnResume(
        IUserCircuit waitingCircuit
    )
    {
        if (waitingCircuit.SessionId is null)
        {
            _logger.LogWarning(
                "Circuit {CircuitId} does not have a session ID.",
                waitingCircuit.Id
            );
            return;
        }

        var sessionId = waitingCircuit.SessionId.Value;

        TaskCompletionSource tcs;
        PausedCircuitReference reference;
        lock (_lock)
        {
            if (!_usersToBePaused.Remove(sessionId, out reference!))
            {
                // This user is not paused
                return;
            }

            // Pause this user

            tcs = new();
            if (!_pausedCircuits.TryAdd(waitingCircuit, tcs))
            {
                _logger.LogError(
                    "Failed to add circuit {CircuitId} to paused circuits.",
                    waitingCircuit.Id
                );
                return;
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Circuit {CircuitId} is paused and waiting.",
                waitingCircuit.Id
            );
        }

        await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(TIMEOUT_MS));

        reference.Unpaused -= PausedCircuitReference_UnpauseUser;
        reference.UnmarkPaused();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Circuit {CircuitId} has finished waiting.",
                waitingCircuit.Id
            );
        }

        lock (_lock)
        {
            if (!_pausedCircuits.Remove(waitingCircuit))
            {
                _logger.LogError(
                    "Failed to remove circuit {CircuitId} from paused circuits.",
                    waitingCircuit.Id
                );
                return;
            }
        }
    }

    public void MirrorCircuitLoaded(
        IMirrorCircuit mirrorCircuit
    )
    {
        var userCircuit = mirrorCircuit.SourceCircuit;
        lock (_lock)
        {
            if (_pausedCircuits.TryGetValue(userCircuit, out var tcs))
            {
                if (!tcs.TrySetResult())
                {
                    _logger.LogError(
                        "Could not resume tcs for circuit {CircuitId}",
                        userCircuit.Id
                    );
                }
            }
        }
    }
}

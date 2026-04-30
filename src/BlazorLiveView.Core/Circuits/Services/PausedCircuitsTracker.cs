using BlazorLiveView.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorLiveView.Core.Circuits.Services;

internal class PausedCircuitsTracker : IPausedCircuitsTracker, IDisposable
{
    private const long TIMEOUT_MS = 10_000;

    private readonly ICircuitTracker _circuitTracker;
    private readonly IOptions<LiveViewOptions> _liveViewOptions;
    private readonly ILogger<PausedCircuitsTracker> _logger;

    private readonly Lock _lock = new();

    /// <summary>
    /// User selector of users whose next circuit will be paused.
    /// Access only with _lock.
    /// </summary>
    private readonly Dictionary<string, PausedCircuitReference> _usersToBePaused = new();

    /// <summary>
    /// 
    /// Access only with _lock.
    /// </summary>
    private readonly Dictionary<IUserCircuit, TaskCompletionSource> _pausedCircuits
        = new();

    public PausedCircuitsTracker(
        ICircuitTracker circuitTracker,
        IOptions<LiveViewOptions> liveViewOptions,
        ILogger<PausedCircuitsTracker> logger
    )
    {
        _circuitTracker = circuitTracker;
        _liveViewOptions = liveViewOptions;
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

    public PausedCircuitReference? PauseUser(
        string userSelector
    )
    {
        lock (_lock)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Pausing user {UserSelector}",
                    userSelector
                );
            }

            PausedCircuitReference reference = new(userSelector);
            reference.Unpaused += PausedCircuitReference_UnpauseUser;
            if (_usersToBePaused.TryAdd(userSelector, reference))
            {
                return reference;
            }
            return null;
        }
    }

    private void PausedCircuitReference_UnpauseUser(PausedCircuitReference reference)
    {
        lock (_lock)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Unpausing user {UserSelector}",
                    reference.userSelector
                );
            }
            reference.Unpaused -= PausedCircuitReference_UnpauseUser;
            _usersToBePaused.Remove(reference.userSelector);
        }
    }

    public async Task WaitOnResume(
        IUserCircuit waitingCircuit
    )
    {
        var userSelector = _liveViewOptions.Value.UserSelectorExtractor(
            waitingCircuit.User
        );

        if (userSelector is null)
        {
            // Do not pause circuits for users that are not logged in
            return;
        }

        TaskCompletionSource tcs;
        PausedCircuitReference reference;
        lock (_lock)
        {
            if (!_usersToBePaused.Remove(userSelector, out var referenceNullable))
            {
                // This user is not paused
                return;
            }
            
            reference = referenceNullable;

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
                "Circuit {CircuitId} is waiting.",
                waitingCircuit.Id
            );
        }

        await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(TIMEOUT_MS));

        reference.Unpaused -= PausedCircuitReference_UnpauseUser;
        reference.Unpause();

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

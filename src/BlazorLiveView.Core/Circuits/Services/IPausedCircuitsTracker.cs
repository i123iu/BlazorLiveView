using BlazorLiveView.Core.Components;

namespace BlazorLiveView.Core.Circuits.Services;

/// <summary>
/// Used to mark sessions (circuits that track their web browser tab) to be
/// paused the next time they are reloaded (refreshed). This can allow mirror
/// circuit to fully load when they are being created.
/// </summary>
public interface IPausedCircuitsTracker
{
    /// <summary>
    /// Mark the circuit with this sessionId to be paused the next time it is
    /// reloaded (refreshed). It will be resumed when a mirror circuit loads
    /// and resumes it by calling <see cref="MirrorCircuitLoaded"/>.
    /// </summary>
    PausedSessionReference? MarkPaused(Guid circuitSessionId);

    /// <summary>
    /// All user circuits (their <see cref="LiveViewWaitOnMirror"/> components)
    /// should call this and resume once this task completes.
    /// </summary>
    Task WaitOnResume(IUserCircuit waitingCircuit);

    /// <summary>
    /// Resumes the source user circuit of this mirror circuit if paused.
    /// </summary>
    void MirrorCircuitLoaded(IMirrorCircuit mirrorCircuit);
}

public class PausedSessionReference(
    Guid circuitSessionId
)
{
    public delegate void StatusChangedHandler(PausedSessionReference reference);
    public event StatusChangedHandler? Unpaused;

    public readonly Guid circuitSessionId = circuitSessionId;
    public bool IsPaused { get; private set; } = true;

    public void UnmarkPaused()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Unpaused?.Invoke(this);
    }

    public override int GetHashCode()
        => circuitSessionId.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is not PausedSessionReference other) return false;
        return circuitSessionId == other.circuitSessionId;
    }
}

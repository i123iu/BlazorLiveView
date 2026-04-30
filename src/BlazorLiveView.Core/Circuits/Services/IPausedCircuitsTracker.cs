using BlazorLiveView.Core.Components;

namespace BlazorLiveView.Core.Circuits.Services;

public interface IPausedCircuitsTracker
{
    /// <summary>
    /// Mark the next circuit created with this userSelector as paused at
    /// start. It will be resumed when a mirror circuit loads and resumes
    /// it by calling <see cref="ResumeUser(IUserCircuit)"/>.
    /// </summary>
    /// <returns>
    /// True when the caller has successfully paused the user.
    /// </returns>
    PausedCircuitReference? PauseUser(string userSelector);

    /// <summary>
    /// All user circuits (their <see cref="LiveViewWaitOnMirror"/> component)
    /// should call this and resume once this task completers.
    /// </summary>
    Task WaitOnResume(IUserCircuit waitingCircuit);

    /// <summary>
    /// Resumes the source user circuit of this mirror circuit if paused.
    /// </summary>
    void MirrorCircuitLoaded(IMirrorCircuit mirrorCircuit);
}

public class PausedCircuitReference(
    string userSelector
)
{
    public delegate void StatusChangedHandler(PausedCircuitReference reference);
    public event StatusChangedHandler? Unpaused;

    public readonly string userSelector = userSelector;
    public bool IsPaused { get; private set; } = true;

    public void Unpause()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Unpaused?.Invoke(this);
    }

    public override int GetHashCode()
        => userSelector.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is not PausedCircuitReference other) return false;
        return userSelector == other.userSelector;
    }
}

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user (admin) that is using <c>BlazorLiveView</c> to view
/// another user's "screen" (browser tab).
/// </summary>
public interface IMirrorCircuit : ICircuit
{
    public delegate void MirrorCircuitBlockedHandler(IMirrorCircuit circuit);

    event MirrorCircuitBlockedHandler? MirrorCircuitBlocked;

    IUserCircuit SourceCircuit { get; }
    IUserCircuit? ParentCircuit { get; }

    /// <summary>
    /// Shoud the mirrored componets be in DebugView?
    /// </summary>
    bool DebugView { get; }

    /// <summary>
    /// When a mirror circuit has been block by this library. E.g. when mirror
    /// circuit tries to change location, which is disallowed. See 
    /// <see cref="BlockReason"/> for the reason. 
    /// </summary>
    bool IsBlocked { get; }

    /// <summary>
    /// Throws an exception if not blocked. 
    /// </summary>
    MirrorCircuitBlockReason BlockReason { get; }

    internal void SetBlocked(MirrorCircuitBlockReason blockReason);
}

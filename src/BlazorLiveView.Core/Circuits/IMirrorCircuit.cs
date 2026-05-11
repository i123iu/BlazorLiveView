using BlazorLiveView.Core.Components.Tools;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user (admin) that is using <c>BlazorLiveView</c> to view
/// another user's "screen" (browser tab).
/// </summary>
public interface IMirrorCircuit : ICircuit
{
    public delegate void MirrorCircuitBlockedHandler(IMirrorCircuit circuit);
    event MirrorCircuitBlockedHandler? MirrorCircuitBlocked;

    public delegate void WindowSizeSyncChangedHandler(IMirrorCircuit circuit);
    event WindowSizeSyncChangedHandler? WindowSizeSyncChanged;

    public delegate void ScrollSyncChangedHandler(IMirrorCircuit circuit);
    event ScrollSyncChangedHandler? ScrollSyncChanged;

    public delegate void PointerSyncChangedHandler(IMirrorCircuit circuit);
    event PointerSyncChangedHandler? PointerSyncChanged;

    public delegate void CursorPositionChangedHandler(IMirrorCircuit circuit);
    event CursorPositionChangedHandler? CursorPositionChanged;

    IUserCircuit SourceCircuit { get; }

    /// <summary>
    /// Unique identifier that is passed as a query parameter to the mirror
    /// endpoint. This way the mirror circuit can be recognized when created.
    /// </summary>
    Guid? State { get; }

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

    bool WindowSizeSyncEnabled { get; }
    bool ScrollSyncEnabled { get; }
    bool LaserPointerEnabled { get; }
    Position? CursorPosition { get; }

    internal void SetBlocked(MirrorCircuitBlockReason blockReason);

    public void NotifyWindowSizeSyncChanged(bool enabled);
    public void NotifyScrollSyncChanged(bool enabled);
    internal void NotifyMirrorCursorChanged(Position? position);
    public void NotifyLaserPointerEnabledChanged(bool enabled);
}

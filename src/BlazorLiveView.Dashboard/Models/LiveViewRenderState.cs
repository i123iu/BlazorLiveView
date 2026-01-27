namespace BlazorLiveView.Dashboard.Models;

internal enum LiveViewRenderState
{
    SourceCircuitNotFound,
    SourceCircuitClosed,
    MirrorCircuitLoading,
    MirrorCircuitBlocked,
    MirrorCircuitClosed,
    Working
}
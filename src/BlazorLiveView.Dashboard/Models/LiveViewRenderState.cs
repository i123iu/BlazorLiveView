namespace BlazorLiveView.Dashboard.Models;

public enum LiveViewRenderState
{
    SourceCircuitNotFound,
    SourceCircuitClosed,
    MirrorCircuitLoading,
    MirrorCircuitBlocked,
    MirrorCircuitClosed,
    Working
}
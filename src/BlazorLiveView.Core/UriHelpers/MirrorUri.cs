namespace BlazorLiveView.Core.UriHelpers;

/// <summary>
/// A URI path for an endpoint that mirrors a source circuit. 
/// See <see cref="ILiveViewMirrorUriBuilder"/>. 
/// </summary>
public readonly struct MirrorUri(string sourceCircuitId, bool debugView = false)
{
    public readonly string sourceCircuitId = sourceCircuitId;
    public readonly bool debugView = debugView;
}
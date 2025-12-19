namespace BlazorLiveView.Core.Circuits;

public readonly struct MirrorCircuitBlockReason(string message)
{
    public readonly string message = message;
}

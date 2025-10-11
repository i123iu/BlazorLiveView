namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user (admin) that is using <c>BlazorLiveView</c> to view
/// another user's "screen" (browser tab).
/// </summary>
public interface IMirrorCircuit : ICircuit
{
    IUserCircuit SourceCircuit { get; }
}

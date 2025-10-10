using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// A connection with a user (admin) that is using <c>BlazorLiveView</c> to view
/// another user's "screen" (browser tab).
/// </summary>
internal interface IMirrorCircuit : ICircuit
{
    IUserCircuit SourceCircuit { get; }

    internal ComponentState GetComponentState(int componentId);
}

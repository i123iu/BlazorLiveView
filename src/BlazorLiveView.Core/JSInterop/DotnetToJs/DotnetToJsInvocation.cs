namespace BlazorLiveView.Core.JSInterop.DotnetToJs;

public readonly record struct DotnetToJsInvocation(
    string Identifier,
    CancellationToken CancellationToken,
    object?[]? Args
);

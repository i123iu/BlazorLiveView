using Microsoft.JSInterop.Infrastructure;

namespace BlazorLiveView.Core.JSInterop;

public static class LiveViewJSRuntimeExtensions
{
    // TODO: calling InvokeVoidAsyncNoMirror in a fire-and-forget manner without
    // discarding the value should not trigger the warning about not awaiting
    // like with IJSRuntime.

    public static async ValueTask InvokeVoidAsyncNoMirror(
        this ILiveViewJSRuntime liveViewJSRuntime,
        string identifier,
        params object?[]? args
    )
    {
        ArgumentNullException.ThrowIfNull(liveViewJSRuntime);

        await liveViewJSRuntime.InvokeAsyncNoMirror<IJSVoidResult>(
            identifier, args
        );
    }

    public static async ValueTask InvokeVoidAsyncNoMirror(
        this ILiveViewJSRuntime liveViewJSRuntime,
        string identifier,
        CancellationToken cancellationToken,
        params object?[]? args
    )
    {
        ArgumentNullException.ThrowIfNull(liveViewJSRuntime);

        await liveViewJSRuntime.InvokeAsyncNoMirror<IJSVoidResult>(
            identifier, cancellationToken, args
        );
    }
}

namespace BlazorLiveView.Core.Options;

public sealed class LiveViewJSInteropOptions
{
    internal bool InterceptIJSRuntime { get; set; } = false;

    /// <summary>
    /// How long to wait before forwarding a JS interop invocation to mirror
    /// circuits. This can be useful for targeting a specific HTML element that
    /// may not be immediately available in the DOM (rendering might sometimes
    /// have a delay).
    /// </summary>
    public TimeSpan DefaultDelayForInvocationForward { get; set; }
        = TimeSpan.FromMilliseconds(100);
}

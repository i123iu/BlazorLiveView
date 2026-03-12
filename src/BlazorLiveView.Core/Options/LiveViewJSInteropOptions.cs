namespace BlazorLiveView.Core.Options;

public sealed class LiveViewJSInteropOptions
{
    /// <summary>
    /// Whether to forward all JS interop invocations to mirror circuit from
    /// any component that injects and uses <see cref="IJSRuntime"/>. To
    /// forward only specific invocations use <see cref="ILiveViewJSRuntime"/>.
    /// </summary>
    public bool InterceptIJSRuntime { get; set; } = true;

    /// <summary>
    /// How long to wait before forwarding a JS interop invocation to mirror
    /// circuits. This can be useful for targeting a specific HTML element that
    /// may not be immediately available in the DOM (rendering might sometimes
    /// have a delay).
    /// </summary>
    public TimeSpan DefaultDelayForInvocationForward { get; set; }
        = TimeSpan.FromMilliseconds(100);
}

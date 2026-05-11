using BlazorLiveView.Core.Components.Tools;
using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Core.JSInterop.ForwardingRules;

namespace BlazorLiveView.Core.Options;

public sealed class LiveViewJSInteropOptions
{
    /// <summary>
    /// The default value of false is overwritten when calling
    /// <see cref="LiveViewExtensions.InterceptIJSRuntime"/>.
    /// </summary>
    internal bool InterceptIJSRuntime { get; set; } = false;

    /// <summary>
    /// Rules for intercepting JS interop invocations and forwarding them to
    /// mirror circuits. Default is to <c>Forward</c> all, with some exceptions
    /// specific to Blazor.
    /// </summary>
    public ForwardingRules DotnetToJsForwardingRules { get; set; } = new(
        ForwardingBehavior.Forward,
        new()
        {
            new ExactForwardingRule("Blazor._internal.PageTitle.getAndRemoveExistingTitle", ForwardingBehavior.Forward),
            new ExactForwardingRule("Blazor._internal.domWrapper.focusBySelector", ForwardingBehavior.Forward),
            new ExactForwardingRule("Blazor._internal.attachWebRendererInterop", ForwardingBehavior.SkipForwarding),
            new ExactForwardingRule("Blazor._internal.navigationManager.enableNavigationInterception", ForwardingBehavior.SkipForwarding),
        }
    );

    /// <summary>
    /// Rules for allowing calls from JS to .NET, most commonly done via
    /// <see cref="DotNetObjectReference"/>. Default is to skip all with some
    /// exceptions for this library's use.
    /// </summary>
    public ForwardingRules JsToDotnetForwardingRules { get; set; } = new(
        ForwardingBehavior.SkipForwarding,
        new()
        {
            new ExactForwardingRule(nameof(LaserPointerTransmitter.BlazorLiveView_LaserPointerTransmitter_OnCursorPosition), ForwardingBehavior.Forward),
            new ExactForwardingRule(nameof(LaserPointerTransmitter.BlazorLiveView_LaserPointerTransmitter_OnCursorExit), ForwardingBehavior.Forward),
        }
    );

    /// <summary>
    /// How long to wait before forwarding a JS interop invocation to mirror
    /// circuits. This can be useful for targeting a specific HTML element that
    /// may not be immediately available in the DOM (rendering might sometimes
    /// have a delay).
    /// </summary>
    public TimeSpan DefaultDelayForInvocationForward { get; set; }
        = TimeSpan.FromMilliseconds(100);

    public LiveViewJSInteropOptions AddMudBlazorForwardingRules()
    {
        DotnetToJsForwardingRules.Rules.Add(new MudBlazorForwardingRule());
        return this;
    }
}

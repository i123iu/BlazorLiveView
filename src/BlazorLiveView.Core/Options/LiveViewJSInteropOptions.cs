using BlazorLiveView.Core.Components.Tools;
using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Core.JSInterop;

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
            new ExactJSInteropForwardingRule("Blazor._internal.PageTitle.getAndRemoveExistingTitle", ForwardingBehavior.Forward),
            new ExactJSInteropForwardingRule("Blazor._internal.domWrapper.focusBySelector", ForwardingBehavior.Forward),
            new ExactJSInteropForwardingRule("Blazor._internal.attachWebRendererInterop", ForwardingBehavior.SkipForwarding),
            new ExactJSInteropForwardingRule("Blazor._internal.navigationManager.enableNavigationInterception", ForwardingBehavior.SkipForwarding),
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
            new ExactJSInteropForwardingRule(nameof(LaserPointerTransmitter.BlazorLiveView_LaserPointerTransmitter_OnCursorPosition), ForwardingBehavior.Forward),
            new ExactJSInteropForwardingRule(nameof(LaserPointerTransmitter.BlazorLiveView_LaserPointerTransmitter_OnCursorExit), ForwardingBehavior.Forward),
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
        DotnetToJsForwardingRules.Rules.Add(new MudBlazorJSInteropForwardingRule());
        return this;
    }
}

public class ForwardingRules(
    ForwardingBehavior defaultBehaviour,
    List<IForwardingRule> rules
)
{
    /// <summary>
    /// The default behavior to apply when no rules match an invocation.
    /// </summary>
    public ForwardingBehavior Default
    { get; set; } = defaultBehaviour;

    /// <summary>
    /// Each rule can specify whether to forward, skip forwarding, or do
    /// nothing and let other rules choose (or the default). Rules are
    /// evaluated in the order of this list.
    /// </summary>
    public List<IForwardingRule> Rules
    { get; set; } = rules;

    public ForwardingBehavior GetForwardingBehavior(string identifier)
    {
        foreach (var rule in Rules)
        {
            var behavior = rule.GetForwardingBehaviour(identifier);
            switch (behavior)
            {
                case RuleForwardingBehavior.Forward:
                    return ForwardingBehavior.Forward;
                case RuleForwardingBehavior.SkipForwarding:
                    return ForwardingBehavior.SkipForwarding;
                case RuleForwardingBehavior.None:
                    continue;
            }
        }

        return Default;
    }
}

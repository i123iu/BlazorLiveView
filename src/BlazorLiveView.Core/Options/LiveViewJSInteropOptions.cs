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
    /// Default behaviour of JS interop invocations interception when no
    /// interception rule matches.
    /// </summary>
    public InterceptionBehavior DefaultInterceptionBehaviour { get; set; }
        = InterceptionBehavior.Intercept;

    /// <summary>
    /// Rules for intercepting JS interop invocations. Each rule can specify
    /// whether to intercept, skip interception, or do nothing and let other
    /// rules choose (or the default). Rules are evaluated in the order of this
    /// list.
    /// </summary>
    public List<IJSInteropInterceptionRule> InterceptionRules { get; set; }
        = new()
        {
            new ExactJSInteropInterceptionRule("Blazor._internal.PageTitle.getAndRemoveExistingTitle", InterceptionBehavior.Intercept),
            new ExactJSInteropInterceptionRule("Blazor._internal.domWrapper.focusBySelector", InterceptionBehavior.Intercept),
            new ExactJSInteropInterceptionRule("Blazor._internal.attachWebRendererInterop", InterceptionBehavior.SkipInterception),
        };

    /// <summary>
    /// How long to wait before forwarding a JS interop invocation to mirror
    /// circuits. This can be useful for targeting a specific HTML element that
    /// may not be immediately available in the DOM (rendering might sometimes
    /// have a delay).
    /// </summary>
    public TimeSpan DefaultDelayForInvocationForward { get; set; }
        = TimeSpan.FromMilliseconds(100);

    public LiveViewJSInteropOptions AddInterceptionRule(IJSInteropInterceptionRule rule)
    {
        InterceptionRules.Add(rule);
        return this;
    }

    public LiveViewJSInteropOptions AddMudBlazorInterceptionRules()
        => AddInterceptionRule(new MudBlazorJSInteropInterceptionRule());
}

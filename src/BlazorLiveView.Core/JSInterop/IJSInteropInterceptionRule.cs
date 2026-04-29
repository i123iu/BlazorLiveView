namespace BlazorLiveView.Core.JSInterop;

public interface IJSInteropInterceptionRule
{
    RuleInterceptionBehavior GetInterceptionBehaviour(string identifier);
}

public enum RuleInterceptionBehavior
{
    /// <summary>
    /// No modification - let other rules or the default behaviour decide.
    /// </summary>
    None,

    /// <summary>
    /// Intercept the current JS interop invocation and forward it to the
    /// mirror circuit no matter the default behaviour.
    /// </summary>
    Intercept,

    /// <summary>
    /// Do not intercept the current JS interop invocation no matter the
    /// default behaviour.
    /// </summary>
    SkipInterception,
}

public enum InterceptionBehavior
{
    /// <summary>
    /// Intercept the current JS interop invocation and forward it to the
    /// mirror circuit.
    /// </summary>
    Intercept,

    /// <summary>
    /// Do not intercept the current JS interop invocation.
    /// </summary>
    SkipInterception,
}

public static class InterceptionBehaviorExtensions
{
    public static RuleInterceptionBehavior ToRuleInterceptionBehavior(
        this InterceptionBehavior interceptionBehavior
    )
    {
        return interceptionBehavior switch
        {
            InterceptionBehavior.Intercept => RuleInterceptionBehavior.Intercept,
            InterceptionBehavior.SkipInterception => RuleInterceptionBehavior.SkipInterception,
            _ => throw new ArgumentOutOfRangeException(nameof(interceptionBehavior), interceptionBehavior, null)
        };
    }
}

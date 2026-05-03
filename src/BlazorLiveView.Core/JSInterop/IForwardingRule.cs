namespace BlazorLiveView.Core.JSInterop;

public interface IForwardingRule
{
    RuleForwardingBehavior GetForwardingBehaviour(string identifier);
}

public enum RuleForwardingBehavior
{
    /// <summary>
    /// No modification - let other rules or the default behaviour decide.
    /// </summary>
    None,

    /// <summary>
    /// Forward the current invocation to the mirror circuit no matter the
    /// default behaviour. In case of calling .NET methods from JS, this means
    /// the method will be invoked when coming from a mirror circuit.
    /// </summary>
    Forward,

    /// <summary>
    /// Do not forward the current invocation to the mirror circuit no matter
    /// the default behaviour. In case of calling .NET methods from JS, this
    /// means the method will be invoked only when coming from a user circuit
    /// (not a mirror circuit).
    /// </summary>
    SkipForwarding,
}

public enum ForwardingBehavior
{
    /// <summary>
    /// Forward the current invocation to the mirror circuit. In case of
    /// calling .NET methods from JS, this means the method will be invoked
    /// when coming from a mirror circuit.
    /// </summary>
    Forward,

    /// <summary>
    /// Do not forward the current invocation to the mirror circuit. In case of
    /// calling .NET methods from JS, this means the method will be invoked
    /// only when coming from a user circuit (not a mirror circuit).
    /// </summary>
    SkipForwarding,
}

public static class ForwardingBehaviorExtensions
{
    public static RuleForwardingBehavior ToRuleForwardingBehavior(
        this ForwardingBehavior forwardingBehavior
    )
    {
        return forwardingBehavior switch
        {
            ForwardingBehavior.Forward => RuleForwardingBehavior.Forward,
            ForwardingBehavior.SkipForwarding => RuleForwardingBehavior.SkipForwarding,
            _ => throw new ArgumentOutOfRangeException(nameof(forwardingBehavior), forwardingBehavior, null)
        };
    }
}

namespace BlazorLiveView.Core.JSInterop.ForwardingRules;

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

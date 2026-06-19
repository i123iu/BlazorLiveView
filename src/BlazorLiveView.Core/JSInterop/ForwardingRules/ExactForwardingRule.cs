namespace BlazorLiveView.Core.JSInterop.ForwardingRules;

/// <summary>
/// A forwarding rule that matches an identifier exactly by string comparison.
/// </summary>
public class ExactForwardingRule(
    string identifier,
    ForwardingBehavior behaviourOnMatch
) : IForwardingRule
{
    public string Identifier { get; } = identifier;
    public ForwardingBehavior BehaviourOnMatch { get; set; } = behaviourOnMatch;

    public RuleForwardingBehavior GetForwardingBehaviour(string identifier)
    {
        return Identifier == identifier
            ? BehaviourOnMatch.ToRuleForwardingBehavior()
            : RuleForwardingBehavior.None;
    }
}

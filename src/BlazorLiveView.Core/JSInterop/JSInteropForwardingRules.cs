namespace BlazorLiveView.Core.JSInterop;

public class ExactJSInteropForwardingRule(
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

public class MudBlazorJSInteropForwardingRule : IForwardingRule
{
    public RuleForwardingBehavior GetForwardingBehaviour(string identifier)
    {
        // TODO: A better way to identify MudBlazor JS functions.
        if (identifier.StartsWith("mud"))
            return RuleForwardingBehavior.SkipForwarding;
        return RuleForwardingBehavior.None;
    }
}

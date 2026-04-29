namespace BlazorLiveView.Core.JSInterop;

public class ExactJSInteropInterceptionRule(
    string identifier,
    InterceptionBehavior interceptionBehaviourOnMatch
) : IJSInteropInterceptionRule
{
    public string Identifier { get; } = identifier;
    public InterceptionBehavior BehaviourOnMatch { get; set; }
        = interceptionBehaviourOnMatch;

    public RuleInterceptionBehavior GetInterceptionBehaviour(string identifier)
    {
        return Identifier == identifier
            ? BehaviourOnMatch.ToRuleInterceptionBehavior()
            : RuleInterceptionBehavior.None;
    }
}

public class MudBlazorJSInteropInterceptionRule : IJSInteropInterceptionRule
{
    public RuleInterceptionBehavior GetInterceptionBehaviour(string identifier)
    {
        // TODO: A better way to identify MudBlazor JS functions.
        if (identifier.StartsWith("mud"))
            return RuleInterceptionBehavior.SkipInterception;
        return RuleInterceptionBehavior.None;
    }
}

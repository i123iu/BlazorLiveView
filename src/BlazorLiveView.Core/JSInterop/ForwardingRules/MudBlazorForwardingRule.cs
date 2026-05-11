namespace BlazorLiveView.Core.JSInterop.ForwardingRules;

public class MudBlazorForwardingRule : IForwardingRule
{
    public RuleForwardingBehavior GetForwardingBehaviour(string identifier)
    {
        // TODO: A better way to identify MudBlazor JS functions.
        if (identifier.StartsWith("mud"))
            return RuleForwardingBehavior.SkipForwarding;
        return RuleForwardingBehavior.None;
    }
}

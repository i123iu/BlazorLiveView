using BlazorLiveView.Core.JSInterop.ForwardingRules;

namespace BlazorLiveView.Sample.Common;

public class DebugDotnetToJsForwardingRule
    : IForwardingRule
{
    public RuleForwardingBehavior GetForwardingBehaviour(string identifier)
    {
        return identifier switch
        {
            "blazorLiveView_logJsInterop" => RuleForwardingBehavior.Forward,
            "blazorLiveView_mirroredFunction" => RuleForwardingBehavior.Forward,
            "blazorLiveView_notMirroredFunction" => RuleForwardingBehavior.SkipForwarding,
            _ => RuleForwardingBehavior.None
        };
    }
}

public class DebugJsToDotnetForwardingRule
    : IForwardingRule
{
    public RuleForwardingBehavior GetForwardingBehaviour(string identifier)
    {
        return RuleForwardingBehavior.None;
    }
}

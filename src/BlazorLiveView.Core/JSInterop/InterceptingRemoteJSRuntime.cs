using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorLiveView.Core.JSInterop;

/// <summary>
/// Wrapper around RemoteJSRuntime that is the default IJSRuntime implementation in Blazor Server.
/// Forwards InvokeAsync calls to mirror circuits, while checking interception rules.
/// </summary>
internal class InterceptingRemoteJSRuntime(
    RemoteJSRuntimeWrapper remoteJSRuntime,
    ICircuitTracker circuitTracker,
    ILogger<InterceptingRemoteJSRuntime> logger,
    IOptions<LiveViewJSInteropOptions> liveViewJSInteropOptions
) : ForwardingJSRuntimeBase(
    remoteJSRuntime,
    circuitTracker,
    logger
)
{
    private readonly IOptions<LiveViewJSInteropOptions> _liveViewJSInteropOptions
        = liveViewJSInteropOptions;

    internal RemoteJSRuntimeWrapper Inner => _remoteJsRuntime;

    protected override bool ShouldForward(string identifier)
    {
        var jsInteropOptions = _liveViewJSInteropOptions.Value;

        foreach (var rule in jsInteropOptions.InterceptionRules)
        {
            switch (rule.GetInterceptionBehaviour(identifier))
            {
                case RuleInterceptionBehavior.Intercept:
                    LogRuleMatchResult(identifier, rule, RuleInterceptionBehavior.Intercept);
                    return true;
                case RuleInterceptionBehavior.SkipInterception:
                    LogRuleMatchResult(identifier, rule, RuleInterceptionBehavior.SkipInterception);
                    return false;
                case RuleInterceptionBehavior.None:
                    continue;
            }
        }


        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "JS interop call '{Identifier}' did not match any interception rules. Applying default {Default}.",
                identifier, jsInteropOptions.DefaultInterceptionBehaviour
            );
        }

        return jsInteropOptions.DefaultInterceptionBehaviour switch
        {
            InterceptionBehavior.Intercept => true,
            InterceptionBehavior.SkipInterception => false,
            _ => throw new Exception("Unimplemented default interception behavior.")
        };
    }

    private void LogRuleMatchResult(string identifier, IJSInteropInterceptionRule rule, RuleInterceptionBehavior behavior)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "JS interop call '{Identifier}' matched interception rule '{RuleType}' with behavior '{Behavior}'.",
                identifier,
                rule.GetType().Name,
                behavior
            );
        }
    }

    public static object ToRemoteJSRuntime(
        InterceptingRemoteJSRuntime intercepted
    )
    {
        return intercepted._remoteJsRuntime.Inner;
    }
}

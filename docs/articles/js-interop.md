# JS interop

BlazorLiveView supports JavaScript interoperability **experimentally**. Making sure the mirror circuit's page has the same state as the user circuit is difficult. For example JS calls are not mirrored by default, since they could cause unexcepted behaviour when invoked on the mirror circuit. 

Mirroring JS calls is possible in two ways:

## Injecting `ILiveViewJSRuntime`

First option is to inject and use `ILiveViewJSRuntime` instead of `IJSRuntime`. This will automatically forward JavaScript invocations (`InvokeAsync`) from the user circuit to all mirror circuits that are currently viewing that session. This also contains the method `InvokeAsyncNoMirror` that would do the same as regular `InvokeAsync`.

For example:

```csharp
@using BlazorLiveView.Core.JSInterop
@inject ILiveViewJSRuntime LiveViewJS

<button @onclick="DoAlert">Click me</button>

@code {
    private void DoAlert()
    {
        _ = LiveViewJS.InvokeVoidAsync("alert", "This will appear on user and all mirror circuits.");
        _ = LiveViewJS.InvokeVoidAsyncNoMirror("alert", "This will appear only on the user circuit.");
    }
}
```

## Intercepting `IJSRuntime`

You can also intercept all calls to the default `IJSRuntime` by using the method `InterceptIJSRuntime` on your `builder.Services`. This will forward all JS calls to mirror circuits, but it will not be possible to invoke JS only on the user circuit. This can be useful for third-party libraries or for code without having to modify it. To enable this, add the following to your `Program.cs`:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime();
```

With this enabled, the following example code will have the JS invocation also mirrored:

```csharp
@inject IJSRuntime JS

<button @onclick="DoAlert">Click me</button>

@code {
    private void DoAlert()
    {
        JS.InvokeVoidAsync("alert", "This will appear on user and all mirror circuits.");
    }
}
```

### Forwarding Rules

To specify which intercepted calls to JS function should be forwarded, you can specify the *forwarding rules* in `DotnetToJsForwardingRules`. When enabling interception, you can provide additional options:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime(options =>
    {
        options.DotnetToJsForwardingRules.Default = ForwardingBehavior.Forward;
        options.DotnetToJsForwardingRules.Rules.Add(new ExactForwardingRule(
            "not_forwarded_function", ForwardingBehavior.SkipForwarding
        ));
    });
```

Rules are evaluated in the order they were added. If no rule matches, the default value will be used. In the example above, all JS calls will be intercepted and forwarded except those to `not_forwarded_function`. You can also create custom rule types by implementing the `IForwardingRule` interface.

### Calling .NET from JavaScript

Calling .NET methods from JavaScript is usually done via `DotNetObjectReference`. To specify which `invokeMethodAsync` calls on this instance should be allowed, you can use the `JsToDotnetForwardingRules`. By default, all calls from .NET to JS are disallowed as they can cause unexpected behavior when invoking methods from the original user circuit. For example, a method on a component could be called once by the user and then "unexpectedly" again by the mirrored invocation.

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime(options =>
    {
        options.JsToDotnetForwardingRules.Default = ForwardingBehavior.SkipForwarding;
        options.JsToDotnetForwardingRules.Rules.Add(new ExactForwardingRule(
            "MethodCalledByMirrorCircuits", ForwardingBehavior.Forward
        ));
    });
```

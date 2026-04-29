# JS interop

BlazorLiveView supports JavaScript interoperability **experimentally**. The DOM state of a page is first built when a mirror session is created, but JS function can be invoked at any time before creation. For this reason, the page state cannot be reconstructed completely. Any JS calls that happen before opening the mirror session will be lost. Mirroring JS calls is possible in two ways:

## Injecting `ILiveViewJSRuntime`

First option is to inject and use `ILiveViewJSRuntime` instead of `IJSRuntime`. This will automatically forward JavaScript invocations from a user circuit to all mirror circuits that are currently viewing that session. This also contains the method `InvokeAsyncNoMirror` that would do the same as regular `InvokeAsync`.

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

You can also intercept all calls to the default `IJSRuntime` by using the method `InterceptIJSRuntime`. This will forward all JS calls to mirror circuits, but it will not be possible to invoke JS only on the user circuit. This can be useful for third-party libraries or for code without having to modify it. To enable this, add the following to your `Program.cs`:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime();
```

For example, with this enabled, the following code will have the JS invocation also mirrored:

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

### Interception Rules

To decide which intercepted calls should be forwarded, you can specify *interception rules*. When enabling interception, you can provide options like this:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime(options =>
    {
        options.DefaultInterceptionBehaviour = InterceptionBehavior.Intercept;
        options.AddInterceptionRule(new ExactJSInteropInterceptionRule(
            "not_intercepted_function", InterceptionBehavior.SkipInterception
        ));
    });
```

Rules are evaluated in the order they were added. If no rule matches, the `DefaultInterceptionBehaviour` will be used. In the example above, all JS calls will be intercepted except those to `not_intercepted_function`. You can also create custom rules by implementing the `IJSInteropInterceptionRule` interface.

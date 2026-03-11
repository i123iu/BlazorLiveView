# JS interop

BlazorLiveView supports JavaScript interoperability **experimentally**. The DOM state of a page is first built when a mirror session is created, but JS function can be invoked at any time before creation. For this reason, the page state cannot be reconstructed completely. Any JS calls that happen before opening the mirror session will be lost.

## `ILiveViewJSRuntime`

First option is to inject and use `ILiveViewJSRuntime` instead of `IJSRuntime`. That automatically forwards JavaScript invocations to all mirror circuits that are currently viewing that user's session.

For example:

```csharp
@using BlazorLiveView.Core.Circuits.Services
@inject ILiveViewJSRuntime LiveViewJS

<button @onclick="DoAlert">Click me</button>

@code {
    private void DoAlert()
    {
        LiveViewJS.InvokeVoidAsync("alert", "This will appear on user and all mirror circuits.");
    }
}
```

## Intercepting all JS invocations

BlazorLiveView also supports intercepting **all** JS interop invocations to the default `IJSRuntime`. To enable this feature, set the flag `InterceptJsInteropInvocations` to `true` when registering BlazorLiveView services like this:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView(options =>
    {
        options.InterceptJsInteropInvocations = true;
    });
```

This way, the example in the previous section would work even when using the default `IJSRuntime`:

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

# MudBlazor

BlazorLiveView supports MudBlazor **experimentally**. MudBlazor uses JavaScript interop a lot, so when using `InterceptIJSRuntime` (see [JS interop](js-interop.md)), you should include the default MudBlazor rules that block MudBlazor JS calls from being mirrored. To do this, use the method `AddMudBlazorInterceptionRules`:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddLiveView()
    .InterceptIJSRuntime(options =>
    {
        options.AddMudBlazorInterceptionRules();
    });
```

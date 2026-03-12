using Microsoft.JSInterop;
using BlazorLiveView.Core.Options;

namespace BlazorLiveView.Core.Circuits.Services;

/// <summary>
/// A subtype of <see cref="IJSRuntime"/> that automatically forwards all JS
/// interop invocations to all mirror circuits that mirror the current user.
/// See <see cref="LiveViewOptions.InterceptJsInteropInvocations"/> for
/// forwarding JS invocations from all <c>IJSRuntime</c>.
/// </summary>
public interface ILiveViewJSRuntime : IJSRuntime
{ }

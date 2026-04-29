using BlazorLiveView.Core.Extensions;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.JSInterop;

/// <summary>
/// A subtype of <see cref="IJSRuntime"/> that automatically forwards all JS
/// interop invocations (<see cref="IJSRuntime.InvokeAsync"/>) to all mirror
/// circuits that are viewing the current session.
/// See <see cref="LiveViewExtensions.InterceptIJSRuntime"/> for
/// forwarding JS invocations from all instances of <c>IJSRuntime</c>.
/// </summary>
public interface ILiveViewJSRuntime : IJSRuntime
{
    /// <summary>
    /// Does not forward the JS interop invocation unlike <c>InvokeAsync</c>.
    /// </summary>
    ValueTask<TValue> InvokeAsyncNoMirror<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, object?[]? args
    );

    /// <summary>
    /// Does not forward the JS interop invocation unlike <c>InvokeAsync</c>.
    /// </summary>
    ValueTask<TValue> InvokeAsyncNoMirror<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(
        string identifier, CancellationToken cancellationToken, object?[]? args
    );
}
